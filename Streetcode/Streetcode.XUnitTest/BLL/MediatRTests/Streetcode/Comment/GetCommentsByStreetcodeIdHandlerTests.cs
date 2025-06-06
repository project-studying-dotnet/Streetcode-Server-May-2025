using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment;

public class GetCommentsByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetCommentsByStreetcodeIdHandler _handler;

    public GetCommentsByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetCommentsByStreetcodeIdHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ReturnsOnlyApprovedComments()
    {
        // Arrange
        var streetcodeId = 1;
        var comments = new List<Comment>
        {
            new Comment { Id = 1, StreetcodeId = streetcodeId, IsApproved = true, ParentCommentId = null, User = new DAL.Entities.Users.User() },
            new Comment { Id = 2, StreetcodeId = streetcodeId, IsApproved = false, ParentCommentId = null, User = new DAL.Entities.Users.User() }
        };
        _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Comment, bool>>>(),
            It.IsAny<System.Func<IQueryable<Comment>, IQueryable<Comment>>>()))
            .ReturnsAsync(comments.Where(c => c.IsApproved).ToList());
        var mapped = new List<CommentDTO> { new CommentDTO { Id = 1, IsApproved = true } };
        _mapperMock.Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<Comment>>())).Returns(mapped);

        // Act
        var result = await _handler.Handle(new GetCommentsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().IsApproved.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsNestedReplies()
    {
        // Arrange
        var streetcodeId = 2;
        var parent = new Comment { Id = 1, StreetcodeId = streetcodeId, IsApproved = true, ParentCommentId = null, User = new DAL.Entities.Users.User(), Replies = new List<Comment>() };
        var reply = new Comment { Id = 2, StreetcodeId = streetcodeId, IsApproved = true, ParentCommentId = 1, User = new DAL.Entities.Users.User() };
        parent.Replies.Add(reply);
        var comments = new List<Comment> { parent, reply };
        _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Comment, bool>>>(),
            It.IsAny<System.Func<IQueryable<Comment>, IQueryable<Comment>>>()))
            .ReturnsAsync(comments);
        var mapped = new List<CommentDTO> {
            new CommentDTO { Id = 1, Replies = new List<CommentDTO> { new CommentDTO { Id = 2 } } }
        };
        _mapperMock.Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<Comment>>())).Returns(mapped);

        // Act
        var result = await _handler.Handle(new GetCommentsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Replies.Should().NotBeNull();
        result.Value.First().Replies.Should().ContainSingle(r => r.Id == 2);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoComments()
    {
        // Arrange
        var streetcodeId = 3;
        _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Comment, bool>>>(),
            It.IsAny<System.Func<IQueryable<Comment>, IQueryable<Comment>>>()))
            .ReturnsAsync(new List<Comment>());
        _mapperMock.Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<Comment>>())).Returns(new List<CommentDTO>());

        // Act
        var result = await _handler.Handle(new GetCommentsByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
} 