using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.GetById;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests;

public class GetNewsByUrlHandlerTests
{
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly GetNewsByUrlHandler _handler;

    public GetNewsByUrlHandlerTests()
    {
        _mockLoggerService = new Mock<ILoggerService>();
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _handler = new GetNewsByUrlHandler(_mockMapper.Object, _mockRepository.Object, _mockBlobService.Object, _mockLoggerService.Object);
    }

    [Fact]
    public async Task Handler_ShouldReturnNewByUrlSuccessfully_NewWtihImage()
    {
        var news = GetNew();
        SetUpMockRepository(news);
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(GetNewsDTO);
        SetUpMockBlobService( "base64string");

        var result = await _handler.Handle(new GetNewsByUrlQuery(news.URL), CancellationToken.None);

        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(r => r.NewsRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<News, bool>>>(),
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldReturnNewByUrlSuccessfully_NewWtihoutImage()
    {
        // Arrange
        var news = GetNew();
        SetUpMockRepository(news);
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(GetNewsDTOWithoutImage());
        SetUpMockBlobService(null);

        // Act
        var result = await _handler.Handle(new GetNewsByUrlQuery(news.URL), CancellationToken.None);

        // Asser
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()), Times.Never);
        _mockRepository.Verify(r => r.NewsRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<News, bool>>>(),
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldReturnError_IncorrectUrl()
    {
        // Arrange
        var news = GetNew();
        var errorMessage = $"No news by entered Url - {news.URL}";
        SetUpMockRepository(news);
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns((NewsDTO)null);
        SetUpMockBlobService(null);

        // Act
        var result = await _handler.Handle(new GetNewsByUrlQuery(news.URL), CancellationToken.None);

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

    private NewsDTO GetNewsDTOWithoutImage()
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

    private void SetUpMockBlobService(string base64String)
    {
        _mockBlobService.Setup(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()))
            .ReturnsAsync(base64String);
    }
}