using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.News;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests;

public class GetNewsAndLinksByUrlHandlerTests
{
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<INewsService> _mockNewsService;
    private readonly GetNewsAndLinksByUrlHandler _handler;

    public GetNewsAndLinksByUrlHandlerTests()
    {
        _mockLoggerService = new Mock<ILoggerService>();
        _mockNewsService = new Mock<INewsService>();
        _handler = new GetNewsAndLinksByUrlHandler(_mockNewsService.Object, _mockLoggerService.Object);
    }

    [Fact]
    public async Task Handler_ShouldReturnNewsWithLinksSuccessfully()
    {
        // Arange
        var url = "/test";
        var newsDtoWithURLs = GetNewsDTOWithURLs();

        _mockNewsService.Setup(x => x.GetNewsWithURLsAsync(url))
            .ReturnsAsync(newsDtoWithURLs);

        // Act
        var result = await _handler.Handle(new GetNewsAndLinksByUrlQuery(url), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockNewsService.Verify(x => x.GetNewsWithURLsAsync(url), Times.Once);
    }

    [Fact]
    public async Task Handler_ShouldReturnError_IncorrectUrl()
    {
        // Arrange
        var url = "/test";
        var errorMessage = $"No news by entered Url - {url}";
        _mockNewsService.Setup(x => x.GetNewsWithURLsAsync(url))
            .ReturnsAsync((NewsDTOWithURLs)null);

        // Act
        var result = await _handler.Handle(new GetNewsAndLinksByUrlQuery(url), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
        _mockNewsService.Verify(x => x.GetNewsWithURLsAsync(url), Times.Once);
        _mockLoggerService.Verify(logger => logger.LogError(It.IsAny<object>(), errorMessage), Times.Once);
    }

    private NewsDTOWithURLs GetNewsDTOWithURLs()
    {
        return new NewsDTOWithURLs
        {
            News = new NewsDTO
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
            },
            PrevNewsUrl = "/test1",
            NextNewsUrl = "/test2",
            RandomNews = new RandomNewsDTO
            {
                Title = "Title",
                RandomNewsUrl = "/test3",
            },
        };
    }
}