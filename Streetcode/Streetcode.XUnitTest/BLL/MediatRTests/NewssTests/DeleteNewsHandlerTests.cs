using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using FluentAssertions;
using FluentResults.Extensions;
using Serilog;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewssTests;

public class DeleteNewsHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _logger;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    
    public DeleteNewsHandlerTests()
    {
        _logger = new Mock<ILoggerService>();
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
    }

    [Fact]
    public async Task ShouldDeleteNewsSuccessfully()
    {
        var testNews = this.GetNews();
        this.SetUpMockRepositoryGetFirstOrDefaultAsync(testNews);
        this.SetUpMockRepositorySaveChangesAsync(1);
        
        var handler = new DeleteNewsHandler(_repositoryWrapper.Object, _logger.Object);
        
        var result = handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);
        
        result.IsCompletedSuccessfully.Should().BeTrue();
    }
    
    [Fact]
    public async Task ShouldReturnErrorMessage_IdIsIncorrect()
    {
        var testNews = this.GetNews();
        var errorMessage = $"No news found by entered Id - {testNews.Id}";
        this.SetUpMockRepositoryGetFirstOrDefaultAsync(null);
        
        var handler = new DeleteNewsHandler(_repositoryWrapper.Object, _logger.Object);
        
        var result = await handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);
        
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    [Fact]
    public async Task ShouldReturnErrorMessage_DeletedFailure()
    {
        var testNews = this.GetNews();
        var errorMessage = "Failed to delete news";
        this.SetUpMockRepositoryGetFirstOrDefaultAsync(testNews);
        this.SetUpMockRepositorySaveChangesAsync(0);
        
        var handler = new DeleteNewsHandler(_repositoryWrapper.Object, _logger.Object);
        
        var result = await handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);
        
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
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

    private void SetUpMockRepositoryGetFirstOrDefaultAsync(News news)
    {
        this._repositoryWrapper.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<News, bool>>>(), null))
            .ReturnsAsync(news);
    }

    private void SetUpMockRepositorySaveChangesAsync(int number)
    {
        this._repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(number);
    }
}