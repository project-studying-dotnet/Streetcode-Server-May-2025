using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using EntAudio = Streetcode.DAL.Entities.Media.Audio;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.GetAll;

public class GetAllAudiosHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repo = new();
    private readonly Mock<IMapper> _map = new();
    private readonly Mock<IBlobService> _blob = new();
    private readonly Mock<ILoggerService> _log = new();
    private readonly GetAllAudiosHandler _handler;

    public GetAllAudiosHandlerTests()
    {
        _handler = new GetAllAudiosHandler(_repo.Object, _map.Object, _blob.Object, _log.Object);
    }

    [Fact] 
    public async Task Handle_WhenSuccessful_ReturnsBase64()
    {
        // Arrange
        var entities = new[] { new EntAudio { Id = 1, BlobName = "a.mp3" } };
        var dtos = new[] { new AudioDTO { Id = 1, BlobName = "a.mp3" } };

        SetupRepository(entities);
        SetupMapper(entities, dtos);
        SetupBlobService("a.mp3", "b64");

        // Act
        var result = await _handler.Handle(new GetAllAudiosQuery(), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Single().Base64.Should().Be("b64");
    }

    [Fact]
    public async Task Handle_WhenRepoReturnsNull_Fails()
    {
        // Arrange
        SetupRepository(null);

        // Act
        var result = await _handler.Handle(new GetAllAudiosQuery(), default);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        SetupRepository(Array.Empty<EntAudio>());
        SetupMapper(Array.Empty<EntAudio>(), Array.Empty<AudioDTO>());

        // Act
        var result = await _handler.Handle(new GetAllAudiosQuery(), default);

        // Assert  
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_CallsBlobService_ForEachAudio()
    {
        // Arrange
        var entities = new[]
        {
            new EntAudio { Id = 1, BlobName = "1.mp3" },
            new EntAudio { Id = 2, BlobName = "2.mp3" }
        };
        var dtos = new[]
        {
            new AudioDTO { Id = 1, BlobName = "1.mp3" },
            new AudioDTO { Id = 2, BlobName = "2.mp3" }
        };

        SetupRepository(entities);
        SetupMapper(entities, dtos);
        _blob.Setup(b => b.FindFileInStorageAsBase64Async(It.IsAny<string>()))
             .ReturnsAsync("b64");

        // Act
        await _handler.Handle(new GetAllAudiosQuery(), default);

        // Assert
        _blob.Verify(b => b.FindFileInStorageAsBase64Async("1.mp3"), Times.Once);
        _blob.Verify(b => b.FindFileInStorageAsBase64Async("2.mp3"), Times.Once);
    }

    private void SetupRepository(IEnumerable<EntAudio>? entities)
    {
        _repo.Setup(r => r.AudioRepository.GetAllAsync(
                It.IsAny<Expression<Func<EntAudio, bool>>>(),
                It.IsAny<Func<IQueryable<EntAudio>, IIncludableQueryable<EntAudio, object>>>()))
             .ReturnsAsync(entities);
    }

    private void SetupMapper(IEnumerable<EntAudio> entities, IEnumerable<AudioDTO> dtos)
    {
        _map.Setup(m => m.Map<IEnumerable<AudioDTO>>(entities))
            .Returns(dtos);
    }

    private void SetupBlobService(string blobName, string base64)
    {
        _blob.Setup(b => b.FindFileInStorageAsBase64Async(blobName))
             .ReturnsAsync(base64);
    }
}