using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.Create;
using Streetcode.BLL.DTO.News;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests;

public class CreateNewsHandlerTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<ILoggerService> _loggerService;
    private readonly CreateNewsHandler _handler;

    public CreateNewsHandlerTests()
    {
        _mapper = new Mock<IMapper>();
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _loggerService = new Mock<ILoggerService>();
        _handler = new CreateNewsHandler(_mapper.Object, _repositoryWrapper.Object, _loggerService.Object);
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
        var errorMessage = "Cannot convert null to news";
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
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("Failed to create a news");
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