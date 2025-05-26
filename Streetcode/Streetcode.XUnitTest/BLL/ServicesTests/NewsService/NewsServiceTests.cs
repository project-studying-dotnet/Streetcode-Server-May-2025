using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Services.News;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.ServicesTests;

public class NewsServiceTests
{
    private readonly NewsService _newsService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IBlobService> _mockBlobService;

    public NewsServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockBlobService = new Mock<IBlobService>();
        _newsService = new NewsService(_mockBlobService.Object, _mockRepoWrapper.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetNewsByUrlAsync_WhenImageExists_ShouldReturnNewsDtoSuccessfully()
    {
        // Arrange
        var url = "/test";
        var newsDto = new NewsDTO
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
        _mockRepoWrapper.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<News, bool>>>(),
                It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()))
            .ReturnsAsync(new News());
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>())).Returns(newsDto);
        var base64String = "base64";
        _mockBlobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns(base64String);

        // Act
        var result = await _newsService.GetNewsByUrlAsync(url);

        // Assert
        result.Should().BeEquivalentTo(newsDto);
        _mockRepoWrapper.Verify(x => x.NewsRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<News, bool>>>(),
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()), Times.Once);
        _mockMapper.Verify(x => x.Map<NewsDTO>(It.IsAny<News>()), Times.Once);
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetNewsByUrlAsync_WhenImageDoesNotExist_ShouldReturnNewsDtoSuccessfully()
    {
        // Arrange
        var url = "/test";
        var newsDto = new NewsDTO
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
        _mockRepoWrapper.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<News, bool>>>(),
                It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()))
            .ReturnsAsync(new News());
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>())).Returns(newsDto);
        var base64String = "base64";
        _mockBlobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns(base64String);

        // Act
        var result = await _newsService.GetNewsByUrlAsync(url);

        // Assert
        result.Should().BeEquivalentTo(newsDto);
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetNewsByUrlAsync_UrlIsIncorrect_ShouldReturnNull()
    {
        // Arrange
        var url = "/test";
        _mockRepoWrapper.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<News, bool>>>(),
                It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>>()))
            .ReturnsAsync(new News());
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>())).Returns((NewsDTO)null);

        // Act
        var result = await _newsService.GetNewsByUrlAsync(url);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNewsWithURLsAsync_WhenNewsMoreThanFour_ShouldReturnNewsDtoWithAllUrls()
    {
        // Arrange
        var url = "/test1";
        var newsDto = new NewsDTO { Id = 2 };
        SetUpGetNewsByUrlAsync(newsDto);
        var news = new List<News>
        {
            new News { Id = 1, URL = "/test" },
            new News { Id = 2, URL = url },
            new News { Id = 3, URL = "/test2" },
            new News { Id = 4, URL = "/test3" },
        };
        SetUpNewsRepositoryGetAllAsync(news);

        // Act
        var newsDTOWithURLs = await _newsService.GetNewsWithURLsAsync(url);

        // Assert
        newsDTOWithURLs.News.Id.Should().Be(newsDto.Id);
        newsDTOWithURLs.PrevNewsUrl.Should().Be("/test");
        newsDTOWithURLs.NextNewsUrl.Should().Be("/test2");
        newsDTOWithURLs.RandomNews.RandomNewsUrl.Should().Be("/test3");
    }

    [Fact]
    public async Task GetNewsWithURLsAsync_WhenThreeNews_ShouldReturnNewsDtoWithoutRandomNew()
    {
        // Arrange
        var url = "/test1";
        var newsDto = new NewsDTO { Id = 2 };
        SetUpGetNewsByUrlAsync(newsDto);
        var news = new List<News>
        {
            new News { Id = 1, URL = "/test" },
            new News { Id = 2, URL = url },
            new News { Id = 3, URL = "/test2" },
        };
        SetUpNewsRepositoryGetAllAsync(news);

        // Act
        var newsDTOWithURLs = await _newsService.GetNewsWithURLsAsync(url);

        // Assert
        newsDTOWithURLs.News.Id.Should().Be(newsDto.Id);
        newsDTOWithURLs.RandomNews.RandomNewsUrl.Should().Be(url);
    }

    [Fact]
    public async Task GetNewsWithURLsAsync_WhenTwoNews_ShouldReturnNewsDtoOnlyWithPrevNew()
    {
        // Arrange
        var url = "/test1";
        var newsDto = new NewsDTO { Id = 2 };
        SetUpGetNewsByUrlAsync(newsDto);
        var news = new List<News>
        {
            new News { Id = 1, URL = "/test" },
            new News { Id = 2, URL = url },
        };
        SetUpNewsRepositoryGetAllAsync(news);

        // Act
        var newsDTOWithURLs = await _newsService.GetNewsWithURLsAsync(url);

        // Assert
        newsDTOWithURLs.News.Id.Should().Be(newsDto.Id);
        newsDTOWithURLs.PrevNewsUrl.Should().Be("/test");
        newsDTOWithURLs.NextNewsUrl.Should().Be(null);
        newsDTOWithURLs.RandomNews.RandomNewsUrl.Should().Be(url);
    }

    [Fact]
    public async Task GetNewsWithURLsAsync_WhenTwoNews_ShouldReturnNewsDtoOnlyWithNextNew()
    {
        // Arrange
        var url = "/test1";
        var newsDto = new NewsDTO { Id = 2 };
        SetUpGetNewsByUrlAsync(newsDto);
        var news = new List<News>
        {
            new News { Id = 2, URL = url },
            new News { Id = 3, URL = "/test2" },
        };
        SetUpNewsRepositoryGetAllAsync(news);

        // Act
        var newsDTOWithURLs = await _newsService.GetNewsWithURLsAsync(url);

        // Assert
        newsDTOWithURLs.News.Id.Should().Be(newsDto.Id);
        newsDTOWithURLs.PrevNewsUrl.Should().Be(null);
        newsDTOWithURLs.NextNewsUrl.Should().Be("/test2");
        newsDTOWithURLs.RandomNews.RandomNewsUrl.Should().Be(url);
    }

    [Fact]
    public async Task GetNewsWithURLsAsync_WhenOneNew_ShouldReturnOnlyCurrentNew()
    {
        // Arrange
        var url = "/test1";
        var newsDto = new NewsDTO { Id = 2 };
        SetUpGetNewsByUrlAsync(newsDto);
        var news = new List<News>
        {
            new News { Id = 2, URL = url },
        };
        SetUpNewsRepositoryGetAllAsync(news);

        // Act
        var newsDTOWithURLs = await _newsService.GetNewsWithURLsAsync(url);

        // Assert
        newsDTOWithURLs.News.Id.Should().Be(newsDto.Id);
        newsDTOWithURLs.PrevNewsUrl.Should().Be(null);
        newsDTOWithURLs.NextNewsUrl.Should().Be(null);
        newsDTOWithURLs.RandomNews.RandomNewsUrl.Should().Be(url);
    }

    private void SetUpGetNewsByUrlAsync(NewsDTO newsDto)
    {
        _mockRepoWrapper.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<System.Func<News, bool>>>(),
                It.IsAny<System.Func<System.Linq.IQueryable<News>, IIncludableQueryable<News, object>>>()))
            .ReturnsAsync(new News());
        _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>())).Returns(newsDto);
    }

    private void SetUpNewsRepositoryGetAllAsync(List<News> news)
    {
        _mockRepoWrapper.Setup(x => x.NewsRepository.GetAllAsync(null, null))
            .ReturnsAsync(news);
    }
}