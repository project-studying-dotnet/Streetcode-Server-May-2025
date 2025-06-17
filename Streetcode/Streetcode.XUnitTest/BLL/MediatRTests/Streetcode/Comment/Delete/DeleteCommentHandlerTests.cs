using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using CommentEntity = Streetcode.DAL.Entities.Streetcode.Comment;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment.Delete;

public class DeleteCommentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly DeleteCommentHandler _handler;

    public DeleteCommentHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new DeleteCommentHandler(_mockRepoWrapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CommentNotFound_ReturnsFailureResult()
    {
        // Arrange
        SetupRepositoryGetComment(null);
        var command = CreateCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CommentNotFound_LogsCorrectError()
    {
        // Arrange
        SetupRepositoryGetComment(null);
        var command = CreateCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(l => l.LogError(
            command,
            It.Is<string>(s => s.Contains($"Cannot find comment with id: {command.Id}"))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CommentNotFound_DoesNotCallDelete()
    {
        // Arrange
        SetupRepositoryGetComment(null);
        var command = CreateCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepoWrapper.Verify(r => r.CommentRepository.Delete(It.IsAny<CommentEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccessResult()
    {
        // Arrange
        SetupRepositoryGetComment(CreateComment());
        SetupSaveChangesAsync(1);
        var command = CreateCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidId_CallsDeleteOnce()
    {
        // Arrange
        var comment = CreateComment();
        SetupRepositoryGetComment(comment);
        SetupSaveChangesAsync(1);
        var command = CreateCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepoWrapper.Verify(r => r.CommentRepository.Delete(comment), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_CallsSaveChangesOnce()
    {
        // Arrange
        SetupRepositoryGetComment(CreateComment());
        SetupSaveChangesAsync(1);
        var command = CreateCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_DoesNotLogError()
    {
        // Arrange
        SetupRepositoryGetComment(CreateComment());
        SetupSaveChangesAsync(1);
        var command = CreateCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    private void SetupRepositoryGetComment(CommentEntity? comment)
    {
        _mockRepoWrapper
            .Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>?>()))
            .ReturnsAsync(comment);
    }

    private void SetupSaveChangesAsync(int result)
    {
        _mockRepoWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(result);
    }

    private DeleteCommentCommand CreateCommand(int commentId = 1)
    {
        return new DeleteCommentCommand(commentId);
    }

    private CommentEntity CreateComment(int id = 1)
    {
        return new CommentEntity { Id = id };
    }
}