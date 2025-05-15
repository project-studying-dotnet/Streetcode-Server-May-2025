using System.Linq.Expressions;
using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.DTO.News;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewssTests;

public class CreateNewsHandlerTests
{
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<ILoggerService> _loggerService;
    
    public CreateNewsHandlerTests()
    {
        this._mapper = new Mock<IMapper>();
        this._repositoryWrapper = new Mock<IRepositoryWrapper>();
        this._loggerService = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task ShouldReturnCorrectType_CorrectInput()
    {
        var newsDto = this.GetNewsDto();
        
        this.SetUpMockMapper();
        this.SetUpMockRepositoryCreate();
        this.SetUpMockRepositorySaveChanges(1);
        
        var handler = new CreateNewsHandler(this._mapper.Object, this._repositoryWrapper.Object, this._loggerService.Object);
        
        var result = await handler.Handle(new CreateNewsCommand(newsDto), CancellationToken.None);

        result.Value.Should().BeOfType<NewsDTO>();
    }

    [Fact]
    public async Task ShouldReturnErrorMessage_IncorrectInput()
    {
        var newsDto = new NewsDTO() { };
        var errorMessage = "Cannot convert null to news";

        this.SetUpMockMapperReturnsNull();
        
        var handler = new CreateNewsHandler(this._mapper.Object, this._repositoryWrapper.Object, this._loggerService.Object);
        
        var result = await handler.Handle(new CreateNewsCommand(newsDto), CancellationToken.None);
        
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    [Fact]
    public async Task ShouldReturnSuccess_CorrectCreatedNews()
    {
        var newsDto = this.GetNewsDto();
        
        this.SetUpMockMapper();
        this.SetUpMockRepositoryCreate();
        this.SetUpMockRepositorySaveChanges(1);
        
        var handler = new CreateNewsHandler(this._mapper.Object, this._repositoryWrapper.Object, this._loggerService.Object);
        
        var result = await handler.Handle(new CreateNewsCommand(newsDto), CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturnErrorMessage_WhenSaveChangesIsFalse()
    {
        var newsDto = this.GetNewsDto();
        
        this.SetUpMockMapper();
        this.SetUpMockRepositoryCreate();
        this.SetUpMockRepositorySaveChanges(0);
        
        var handler = new CreateNewsHandler(this._mapper.Object, this._repositoryWrapper.Object, this._loggerService.Object);
        
        var result = await handler.Handle(new CreateNewsCommand(newsDto), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be("Failed to create a news");
    }

    private NewsDTO GetNewsDto()
    {
        return new NewsDTO()
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
        return new News()
        {
            Id = 1,
            Title = "Title",
            Text = "Text",
            ImageId = 1,
            URL = "/test",
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private News GetIncorectNews()
    {
        return new News() { };
    }

    private void SetUpMockMapper()
    {
        this._mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(this.GetNews());
        this._mapper.Setup(x => x.Map<NewsDTO>(It.IsAny<News>()))
            .Returns(this.GetNewsDto());
    }

    private void SetUpMockMapperReturnsNull()
    {
        this._mapper.Setup(x => x.Map<News>(It.IsAny<NewsDTO>()))
            .Returns(this.GetIncorectNews);
    }

    private void SetUpMockRepositorySaveChanges(int number)
    {
        this._repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(number);
    }

    private void SetUpMockRepositoryCreate()
    {
        this._repositoryWrapper.Setup(x => x.NewsRepository.Create(It.IsAny<News>()))
            .Returns(this.GetNews());
    }
}