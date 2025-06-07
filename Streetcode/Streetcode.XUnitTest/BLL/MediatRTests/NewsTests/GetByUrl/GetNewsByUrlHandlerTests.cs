using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.News;
using Streetcode.BLL.MediatR.News.GetByUrl;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests.GetByUrl;

public class GetNewsByUrlHandlerTests
{
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<INewsService> _mockNewsService;
    private readonly Mock<IStringLocalizer<GetNewsByUrlHandler>> _localizerMock;
    private readonly GetNewsByUrlHandler _handler;

    public GetNewsByUrlHandlerTests()
    {
        _mockLoggerService = new Mock<ILoggerService>();
        _mockNewsService = new Mock<INewsService>();
        _localizerMock = new Mock<IStringLocalizer<GetNewsByUrlHandler>>();
        _handler = new GetNewsByUrlHandler(_mockLoggerService.Object,
            _mockNewsService.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task Handler_CorrectUrl_ShouldReturnNewByUrlSuccess()
    {
        // Arange
        var url = "/test";
        var newsDto = GetNewsDTO();

        _mockNewsService.Setup(x => x.GetNewsByUrlAsync(url))
            .ReturnsAsync(newsDto);

        // Act
        var result = await _handler.Handle(new GetNewsByUrlQuery(url), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockNewsService.Verify(x => x.GetNewsByUrlAsync(url), Times.Once);
    }

    [Fact]
    public async Task Handler_IncorrectUrl_ShouldReturnError()
    {
        // Arrange
        var url = "/test";
        var localized = new LocalizedString(
            "NoNewsByEnteredUrl",
            $"No news by entered Url - {url}"
        );
        _localizerMock
            .Setup(l => l["NoNewsByEnteredUrl", url])
            .Returns(localized);

        var errorMessage = localized.Value;
        _mockNewsService.Setup(x => x.GetNewsByUrlAsync(url))
            .ReturnsAsync((NewsDTO)null);

        // Act
        var result = await _handler.Handle(new GetNewsByUrlQuery(url), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
        _mockNewsService.Verify(x => x.GetNewsByUrlAsync(url), Times.Once);
        _mockLoggerService.Verify(logger => logger.LogError(It.IsAny<object>(), errorMessage), Times.Once);
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
}

