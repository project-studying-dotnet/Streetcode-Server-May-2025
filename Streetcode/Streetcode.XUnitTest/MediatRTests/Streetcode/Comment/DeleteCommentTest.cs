using FluentResults;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Comment;

public class DeleteCommentTest
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<ILoggerService> _mockLogger;

    public DeleteCommentTest()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccessResult()
    {
        // Arrange
        var comment = new Comment { Id = 1, Text = "Test comment" };

        _mockRepo.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Func<Comment, bool>>()))
            .ReturnsAsync(comment);

        _mockRepo.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        var handler = new DeleteCommentHandler(_mockRepo.Object, _mockLogger.Object);
        var command = new DeleteCommentCommand(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.CommentRepository.Delete(comment), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidId_ReturnsFailureResult()
    {
        // Arrange
        _mockRepo.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Func<Comment, bool>>()))
            .ReturnsAsync((Comment)null);

        var handler = new DeleteCommentHandler(_mockRepo.Object, _mockLogger.Object);
        var command = new DeleteCommentCommand(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        _mockRepo.Verify(r => r.CommentRepository.Delete(It.IsAny<Comment>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
} 