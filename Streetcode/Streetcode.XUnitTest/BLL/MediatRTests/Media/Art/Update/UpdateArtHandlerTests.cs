using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Repositories.Interfaces;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using FluentAssertions;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.Update
{
    public class UpdateArtHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IArtRepository> _mockArtRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IBlobService> _mockBlobService;
        private readonly Mock<ILoggerService> _mockLoggerService;
        private readonly UpdateArtHandler _handler;

        public UpdateArtHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockArtRepository = new Mock<IArtRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockBlobService = new Mock<IBlobService>();
            _mockLoggerService = new Mock<ILoggerService>();

            _mockRepositoryWrapper.Setup(r => r.ArtRepository).Returns(_mockArtRepository.Object);

            _handler = new UpdateArtHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockBlobService.Object,
                _mockLoggerService.Object
            );
        }

        [Fact]
        public async Task Handle_ValidUpdateRequest_ReturnsOkResultWithUpdatedArtDTO()
        {
            // Arrange
            var artUpdateRequest = new UpdateArtRequestDTO { Id = 1, Title = "Updated Title", Description = "Updated Description" };
            var command = new UpdateArtCommand(artUpdateRequest);
            var existingArtEntity = new ArtEntity { Id = 1, Title = "Old Title", Description = "Old Desc", ImageId = 1, Image = new ImageEntity { Id = 1, BlobName = "image.png" } };
            var updatedArtDto = new ArtDTO { Id = 1, Title = "Updated Title", Description = "Updated Description", Image = new ImageDTO { BlobName = "image.png", Base64 = "base64image" } };

            _mockArtRepository.Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(existingArtEntity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<ArtDTO>(It.Is<ArtEntity>(a => a.Id == 1 && a.Title == "Updated Title" && a.Description == "Updated Description")))
                .Returns(updatedArtDto);

            _mockBlobService.Setup(s => s.FindFileInStorageAsBase64Async("image.png")).ReturnsAsync("base64image");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(updatedArtDto);
            _mockArtRepository.Verify(r => r.Update(It.Is<ArtEntity>(a => a.Title == "Updated Title" && a.Description == "Updated Description")), Times.Once);
        }

        [Fact]
        public async Task Handle_ArtNotFoundForUpdate_ReturnsFailResult()
        {
            // Arrange
            var artUpdateRequest = new UpdateArtRequestDTO { Id = 99, Title = "Non Existent" };
            var command = new UpdateArtCommand(artUpdateRequest);

            _mockArtRepository.Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync((ArtEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain($"Art with ID {artUpdateRequest.Id} not found.");
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncFailsDuringUpdate_ReturnsFailResult()
        {
            // Arrange
            var artUpdateRequest = new UpdateArtRequestDTO { Id = 1, Title = "Updated Title" };
            var command = new UpdateArtCommand(artUpdateRequest);
            var existingArtEntity = new ArtEntity { Id = 1, Title = "Old Title", ImageId = 1, Image = new ImageEntity { Id = 1, BlobName = "image.png" } };

            _mockArtRepository.Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(existingArtEntity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.First().Message.Should().Contain($"Failed to update Art with ID {artUpdateRequest.Id}");
        }

        [Fact]
        public async Task Handle_BlobServiceThrowsExceptionWhenFetchingBase64_ThrowsException()
        {
            // Arrange
            var artUpdateRequest = new UpdateArtRequestDTO { Id = 1, Title = "Updated Title" };
            var command = new UpdateArtCommand(artUpdateRequest);

            var existingArtEntity = new ArtEntity
            {
                Id = 1,
                Title = "Old Title",
                ImageId = 1,
                Image = new ImageEntity { Id = 1, BlobName = "image.png" }
            };

            _mockArtRepository.Setup(r => r.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                .ReturnsAsync(existingArtEntity);

            _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mockMapper.Setup(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()))
                .Returns(new ArtDTO
                {
                    Id = 1,
                    Title = "Updated Title",
                    Image = new ImageDTO { BlobName = "image.png" }
                });

            var expectedExceptionMessage = "Failed to fetch Base64";
            _mockBlobService.Setup(s => s.FindFileInStorageAsBase64Async("image.png"))
                .ThrowsAsync(new InvalidOperationException(expectedExceptionMessage));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            (await act.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(expectedExceptionMessage);
        }
    }
}
