using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.GetById;
using Streetcode.BLL.MediatR.News.GetById;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests;

public class GetNewsByIdHandlerTests
{
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly GetNewsByIdHandler _handler;

    public GetNewsByIdHandlerTests()
    {
        _mockLoggerService = new Mock<ILoggerService>();
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _handler = new GetNewsByIdHandler(_mockMapper.Object, _mockRepository.Object, _mockBlobService.Object, _mockLoggerService.Object);
    }

    [Fact]
    public async Task Handle_CorrectId_ShouldReturnNewSuccesfully()
    {
        // Arrange
        var news = GetNew();
        SetUpMockRepository(news);
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(GetNewsDTO);
        SetUpMockBlobService( "base64string");

        // Act
        var result = await _handler.Handle(new GetNewsByIdQuery(news.Id), CancellationToken.None);

        // Assert
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(x => x.NewsRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<News, bool>>>(),
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle__NewWithoutImage_ShouldReturnNewSucces()
    {
        // Arrange
        var news = GetNew();
        SetUpMockRepository(news);
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(GetNewsDTOWithouImage);
        SetUpMockBlobService(null);

        // Act
        var result = await _handler.Handle(new GetNewsByIdQuery(news.Id), CancellationToken.None);

        // Assert
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(x => x.NewsRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<News, bool>>>(),
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MappingReturnNullDto_ShouldReturnError()
    {
        // Arrange
        var news = GetNew();
        var errorMessage = $"No news by entered Id - {news.Id}";
        SetUpMockRepository(news);
        SetUpMockMapperWithNull();
        SetUpMockBlobService(null);

        // Act
        var result = await _handler.Handle(new GetNewsByIdQuery(news.Id), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    private News GetNew()
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

    private NewsDTO GetNewsDTO()
    {
        return new NewsDTO
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            Image = new ImageDTO
            {
                Id = 1,
                BlobName = "testblob",
                MimeType = "image/png",
            },
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private NewsDTO GetNewsDTOWithouImage()
    {
        return new NewsDTO
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private void SetUpMockRepository(News news)
    {
        _mockRepository.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<News, bool>>>(),
                It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()))
            .ReturnsAsync(news);
    }

    private void SetUpMockMapperWithNull()
    {
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns((NewsDTO)null);
    }

    private void SetUpMockBlobService(string base64String)
    {
        _mockBlobService.Setup(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()))
            .ReturnsAsync(base64String);
    }
}