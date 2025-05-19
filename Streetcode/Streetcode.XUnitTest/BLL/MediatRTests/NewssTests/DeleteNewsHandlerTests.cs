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
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewssTests;

public class DeleteNewsHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _logger;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly DeleteNewsHandler _handler;

    public DeleteNewsHandlerTests()
    {
        _logger = new Mock<ILoggerService>();
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _handler = new DeleteNewsHandler(_repositoryWrapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handler_ShouldDeleteNewsSuccessfully_WhenImageExists()
    {
        // Arrange
        var testNews = GetNews();
        SetUpMockRepositoryGetFirstOrDefaultAsync(testNews);
        _repositoryWrapper.Setup(r => r.NewsRepository.Delete(testNews));
        _repositoryWrapper.Setup(r => r.ImageRepository.Delete(testNews.Image));
        SetUpMockRepositorySaveChangesAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(r => r.ImageRepository.Delete(testNews.Image), Times.Once);
        _repositoryWrapper.Verify(r => r.NewsRepository.Delete(testNews), Times.Once);
        _repositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldDeleteNewsSuccessfully_WhenImageDoesntExists()
    {
        // Arrange
        var testNews = GetNewsWithoutImage();
        SetUpMockRepositoryGetFirstOrDefaultAsync(testNews);
        _repositoryWrapper.Setup(r => r.NewsRepository.Delete(testNews));
        _repositoryWrapper.Setup(r => r.ImageRepository.Delete(testNews.Image));
        SetUpMockRepositorySaveChangesAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(r => r.ImageRepository.Delete(testNews.Image), Times.Never);
        _repositoryWrapper.Verify(r => r.NewsRepository.Delete(testNews), Times.Once);
        _repositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldReturnErrorMessage_IdIsIncorrect()
    {
        // Arrange
        var testNews = GetNews();
        var errorMessage = $"No news found by entered Id - {testNews.Id}";
        SetUpMockRepositoryGetFirstOrDefaultAsync(null);

        // Act
        var result = await _handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    [Fact]
    public async Task Handler_ShouldReturnErrorMessage_DeletedFailure()
    {
        // Arrange
        var testNews = GetNews();
        var errorMessage = "Failed to delete news";
        SetUpMockRepositoryGetFirstOrDefaultAsync(testNews);
        _repositoryWrapper.Setup(r => r.ImageRepository.Delete(testNews.Image));
        _repositoryWrapper.Setup(r => r.NewsRepository.Delete(testNews));
        SetUpMockRepositorySaveChangesAsync(0);

        // Act
        var result = await _handler.Handle(new DeleteNewsCommand(testNews.Id), CancellationToken.None);

        // Assert
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
            Image = new Image
            {
                Id = 1,
                BlobName = "test.jpg",
                MimeType = "image/jpeg",
            },
            CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    private News GetNewsWithoutImage()
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
        _repositoryWrapper.Setup(x => x.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<News, bool>>>(), null))
            .ReturnsAsync(news);
    }

    private void SetUpMockRepositorySaveChangesAsync(int number)
    {
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(number);
    }
}