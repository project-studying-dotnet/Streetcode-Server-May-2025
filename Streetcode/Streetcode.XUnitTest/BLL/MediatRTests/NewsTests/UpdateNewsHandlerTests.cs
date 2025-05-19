using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.Create;
using Streetcode.BLL.MediatR.News.Update;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests;

public class UpdateNewsHandlerTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IBlobService> _blobService;
    private readonly Mock<ILoggerService> _loggerService;
    private readonly UpdateNewsHandler _handler;

    public UpdateNewsHandlerTests()
    {
        _mapper = new Mock<IMapper>();
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _blobService = new Mock<IBlobService>();
        _loggerService = new Mock<ILoggerService>();
        _handler = new UpdateNewsHandler(_repositoryWrapper.Object, _mapper.Object, _blobService.Object, _loggerService.Object);
    }

    [Fact]
    public async Task Handler_ShouldUpdateNewsSuccessfully_WithImage()
    {
        // Arrange
        var newsDto = GetNewsDto();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(GetNews());
        _mapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(newsDto);
        var string64Base = "base64String";
        SetUpMockBlobService(string64Base);
        _repositoryWrapper.Setup(x => x.NewsRepository.Update(It.IsAny<News>()));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new UpdateNewsCommand(newsDto), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(x => x.NewsRepository.Update(It.IsAny<News>()), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
        result.Value.Image.Base64.Should().Be(string64Base);
    }

    /// <summary>
    /// Image exist in Database, so we call Delete method.
    /// </summary>
    [Fact]
    public async Task Handler_ShouldUpdateNewsSuccessfully_NewWithoutImage1()
    {
        // Arrange
        var newsDto = GetNewsDto();
        var image = GetImage();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(GetNewsWithoutImage());
        _mapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(GetNewsDto());
        _repositoryWrapper.Setup(r =>
            r.ImageRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Image, bool>>>(), null))
            .ReturnsAsync(image);
        _repositoryWrapper.Setup(x => x.ImageRepository.Delete(It.IsAny<Image>()));
        _repositoryWrapper.Setup(x => x.NewsRepository.Update(It.IsAny<News>()));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new UpdateNewsCommand(newsDto), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(x => x.ImageRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Image, bool>>>(), null), Times.Once);
        _repositoryWrapper.Verify(x => x.ImageRepository.Delete(It.Is<Image>(img => img == image)), Times.Once);
        _repositoryWrapper.Verify(x => x.NewsRepository.Update(It.IsAny<News>()), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// Image Doesn't exist in Database, so we don't call Delete method.
    /// </summary>
    [Fact]
    public async Task Handler_ShouldUpdateNewsSuccessfully_NewWithoutImage2()
    {
        // Arrange
        var newsDto = GetNewsDto();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(GetNewsWithoutImage());
        _mapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(GetNewsDto());
        _repositoryWrapper.Setup(x => x.ImageRepository.Delete(It.IsAny<Image>()));
        _repositoryWrapper.Setup(x => x.NewsRepository.Update(It.IsAny<News>()));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new UpdateNewsCommand(newsDto), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(x => x.ImageRepository.Delete(It.IsAny<Image>()), Times.Never);
        _repositoryWrapper.Verify(x => x.NewsRepository.Update(It.IsAny<News>()), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldReturnError_WhenNewsNull()
    {
        var errorMessage = "Cannot convert null to news";
        var newsDto = new NewsDTO();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns((News)null);

        var result = await _handler.Handle(new UpdateNewsCommand(newsDto), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    [Fact]
    public async Task Handler_ShouldReturnError_ResultIsFailed()
    {
        // Arrange
        var errorMessage = "Failed to update news";
        var newsDto = GetNewsDto();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(GetNews());
        _mapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(newsDto);
        var string64Base = "base64String";
        SetUpMockBlobService(string64Base);
        _repositoryWrapper.Setup(x => x.NewsRepository.Update(It.IsAny<News>()));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new UpdateNewsCommand(newsDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    private NewsDTO GetNewsDto()
    {
        return new NewsDTO
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            Image = new ImageDTO()
            {
                Id = 1,
                BlobName = "testblob",
                MimeType = "image/png",
            },
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private News GetNews()
    {
        return new News
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            Image = new Image()
            {
                Id = 1,
                BlobName = "testblob",
                MimeType = "image/png",
            },
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private News GetNewsWithoutImage()
    {
        return new News
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private Image GetImage()
    {
        return new Image
        {
            Id = 1,
            BlobName = "testblob",
            MimeType = "image/png",
        };
    }

    private void SetUpMockBlobService(string base64String)
    {
        _blobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns(base64String);
    }
}