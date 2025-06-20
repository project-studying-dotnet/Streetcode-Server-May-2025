using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.Comment;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using CommentEntity = Streetcode.DAL.Entities.Streetcode.Comment;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment.GetById;

public class GetCommentByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetCommentByIdHandler _handler;

    public GetCommentByIdHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetCommentByIdHandler(_mockRepoWrapper.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_CommentNotFound_ReturnsFailureResult()
    {
        // Arrange
        SetupRepositoryGetComment(null);
        var query = CreateQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CommentNotFound_LogsCorrectError()
    {
        // Arrange
        SetupRepositoryGetComment(null);
        var query = CreateQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(l => l.LogError(
            query,
            It.Is<string>(s => s.Contains($"Cannot find comment with id: {query.Id}"))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccessResult()
    {
        // Arrange
        var comment = CreateComment();
        var commentDto = CreateCommentDto();
        SetupRepositoryGetComment(comment);
        SetupMapper(comment, commentDto);
        var query = CreateQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(commentDto);
    }

    [Fact]
    public async Task Handle_ValidId_DoesNotLogError()
    {
        // Arrange
        var comment = CreateComment();
        var commentDto = CreateCommentDto();
        SetupRepositoryGetComment(comment);
        SetupMapper(comment, commentDto);
        var query = CreateQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

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

    private void SetupMapper(CommentEntity comment, AdminCommentDTO commentDto)
    {
        _mockMapper
            .Setup(m => m.Map<AdminCommentDTO>(comment))
            .Returns(commentDto);
    }

    private GetCommentByIdQuery CreateQuery(int commentId = 1)
    {
        return new GetCommentByIdQuery(commentId);
    }

    private CommentEntity CreateComment(int id = 1)
    {
        return new CommentEntity { Id = id };
    }

    private AdminCommentDTO CreateCommentDto(int id = 1)
    {
        return new AdminCommentDTO { Id = id };
    }
} 