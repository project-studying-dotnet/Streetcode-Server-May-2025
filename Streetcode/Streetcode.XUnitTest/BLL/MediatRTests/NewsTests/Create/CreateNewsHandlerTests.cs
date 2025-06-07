using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.Create;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests.Create;

public class CreateNewsHandlerTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<ILoggerService> _loggerService;
    private readonly Mock<IStringLocalizer<CreateNewsHandler>> _localizer;
    private readonly CreateNewsHandler _handler;

    public CreateNewsHandlerTests()
    {
        _mapper = new Mock<IMapper>();
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _loggerService = new Mock<ILoggerService>();
        _localizer = new Mock<IStringLocalizer<CreateNewsHandler>>();
        _handler = new CreateNewsHandler(_mapper.Object,
            _repositoryWrapper.Object,
            _loggerService.Object,
            _localizer.Object);
    }

    [Fact]
    public async Task Handler_CorrectInput_ShouldReturnCorrectType()
    {
        // Arrange
        var news = GetNews();
        var newsDto = GetNewsDto();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(news);
        _mapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(newsDto);
        _repositoryWrapper.Setup(x => x.NewsRepository.CreateAsync(news))
            .ReturnsAsync(news);
        SetUpMockRepositorySaveChanges(1);

        // Act
        var result = await _handler.Handle(new CreateNewsCommand(GetNewsDto()), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(x => x.NewsRepository.CreateAsync(news), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
        result.Value.Should().BeEquivalentTo(newsDto);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_IncorrectInput_ShouldReturnErrorMessage()
    {
        // Arrange
        var newsDto = new NewsDTO();

        var localized = new LocalizedString(
            "CannotConvertNullToNews",
            "Cannot convert null to news"
        );
        _localizer
            .Setup(l => l["CannotConvertNullToNews"])
            .Returns(localized);

        var errorMessage = localized.Value;

        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns((News)null);

        // Act
        var result = await _handler.Handle(new CreateNewsCommand(newsDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    [Fact]
    public async Task Handler_WhenSaveChangesIsFalse_ShouldReturnErrorMessage()
    {
        // Arrange
        var localized = new LocalizedString(
            "FailedToCreateNews",
            "Failed to create a news"
        );
        _localizer
            .Setup(l => l["FailedToCreateNews"])
            .Returns(localized);

        var errorMessage = localized.Value;

        var news = GetNews();
        _mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(news);
        _repositoryWrapper.Setup(x => x.NewsRepository.Create(news))
            .Returns(news);
        SetUpMockRepositorySaveChanges(0);

        // Act
        var result = await _handler.Handle(new CreateNewsCommand(GetNewsDto()), CancellationToken.None);

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
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private void SetUpMockRepositorySaveChanges(int number)
    {
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(number);
    }
}