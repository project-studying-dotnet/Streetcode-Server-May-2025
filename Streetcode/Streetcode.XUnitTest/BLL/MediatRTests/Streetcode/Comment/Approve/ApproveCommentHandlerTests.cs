using FluentAssertions;
using Moq;
using Xunit;
using Streetcode.BLL.MediatR.Streetcode.Comment.Approve;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using CommentEntity = Streetcode.DAL.Entities.Streetcode.Comment;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment.Approve;

public class ApproveCommentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly ApproveCommentHandler _handler;

    public ApproveCommentHandlerTests()
    {
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new ApproveCommentHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldApproveComment_WhenCommentExists()
    {
        // Arrange
        var comment = new CommentEntity { Id = 1, IsApproved = false };
        _repositoryMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CommentEntity, bool>>>(),
            null)).ReturnsAsync(comment);
        _repositoryMock.Setup(r => r.CommentRepository.Update(comment));
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var command = new ApproveCommentCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.IsApproved.Should().BeTrue();
        _repositoryMock.Verify(r => r.CommentRepository.Update(comment), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenCommentNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<CommentEntity, bool>>>(),
            null)).ReturnsAsync((CommentEntity?)null);
        var command = new ApproveCommentCommand(99);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        _repositoryMock.Verify(r => r.CommentRepository.Update(It.IsAny<CommentEntity>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        _loggerMock.Verify(l => l.LogError(command, It.Is<string>(msg => msg.Contains("Cannot find comment"))), Times.Once);
    }
}
