using Xunit;
using Moq;
using FluentAssertions;
using AutoMapper;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetPending;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Interfaces.Logging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Streetcode.DAL.Entities.Users;

using commentEntity = Streetcode.DAL.Entities.Streetcode.Comment;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment.GetPending;

public class GetPendingCommentsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetPendingCommentsHandler _handler;

    public GetPendingCommentsHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetPendingCommentsHandler(_mockRepoWrapper.Object, _mapperMock.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_PendingCommentsExist_ReturnsOkResult()
    {
        // Arrange
        var comments = new List<commentEntity>
        {
            new commentEntity { Id = 1, IsApproved = false, CreatedAt = DateTime.UtcNow, User = new User(), Streetcode = new StreetcodeContent() }
        };

        var adminCommentDtos = new List<AdminCommentDTO>
        {
            new AdminCommentDTO { Id = 1 }
        };

        _mockRepoWrapper.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<commentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<commentEntity>, IIncludableQueryable<commentEntity, object>>>()
        )).ReturnsAsync(comments);

        _mapperMock.Setup(m => m.Map<List<AdminCommentDTO>>(It.IsAny<List<commentEntity>>()))
                   .Returns(adminCommentDtos);

        var request = new GetPendingCommentsQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(adminCommentDtos);
    }

    [Fact]
    public async Task Handle_NullPendingComments_ReturnsFailResult()
    {
        // Arrange
        _mockRepoWrapper.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<commentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<commentEntity>, IIncludableQueryable<commentEntity, object>>>()
        )).ReturnsAsync((List<commentEntity>)null);

        var request = new GetPendingCommentsQuery();

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Cannot find any pending comments"));
    }
}
