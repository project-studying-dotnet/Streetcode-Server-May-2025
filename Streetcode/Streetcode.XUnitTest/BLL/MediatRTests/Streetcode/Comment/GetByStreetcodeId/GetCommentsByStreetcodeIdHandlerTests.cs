using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment.GetByStreetcodeId;

public class GetCommentsByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetCommentsByStreetcodeIdHandler _handler;

    public GetCommentsByStreetcodeIdHandlerTests()
    {
        _handler = new GetCommentsByStreetcodeIdHandler(
            _repositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOkResult_WhenApprovedRootCommentsExist()
    {
        // Arrange
        var (allComments, approvedRootDtos, _) = CreateCommentsWithReplies();
        SetupRepositoryReturn(allComments);
        SetupMapperReturn(approvedRootDtos);

        var query = new GetCommentsByStreetcodeIdQuery(33);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(approvedRootDtos);
        VerifyRepositoryCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldReturnBothApprovedAndUnapproved_WhenForModerationIsTrue()
    {
        // Arrange
        var (allComments, _, allRootDtos) = CreateCommentsWithReplies(includeUnapproved: true);
        SetupRepositoryReturn(allComments);
        SetupMapperReturn(allRootDtos);

        var query = new GetCommentsByStreetcodeIdQuery(33, true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(allRootDtos);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCommentsExist()
    {
        // Arrange
        SetupRepositoryReturn(Array.Empty<DAL.Entities.Streetcode.Comment>());
        _mapperMock.Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<DAL.Entities.Streetcode.Comment>>()))
                   .Returns(new List<CommentDTO>());

        var query = new GetCommentsByStreetcodeIdQuery(99);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        VerifyRepositoryCalledOnce();
    }

    [Fact]
    public async Task Handle_ShouldMapOnlyRootComments_AndExcludeReplies()
    {
        // Arrange
        var (allComments, approvedRootDtos, _) = CreateCommentsWithReplies();
        SetupRepositoryReturn(allComments);
        
        List<DAL.Entities.Streetcode.Comment>? listPassedToMapper = null;
        _mapperMock
            .Setup(m => m.Map<List<CommentDTO>>(It.IsAny<object>()))
            .Callback<object>(src => listPassedToMapper = src as List<DAL.Entities.Streetcode.Comment>)
            .Returns(approvedRootDtos);

        var query = new GetCommentsByStreetcodeIdQuery(33);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        listPassedToMapper.Should().NotBeNull();
        listPassedToMapper!.Should().OnlyContain(c => c.ParentCommentId == null);
    }

    [Fact]
    public async Task Handle_ShouldFilterOutUnapproved_WhenForModerationFalse()
    {
        // Arrange
        var (allComments, approvedRootDtos, _) = CreateCommentsWithReplies(includeUnapproved: true);
        Expression<Func<DAL.Entities.Streetcode.Comment, bool>>? capturedPredicate = null;

        _repositoryMock
            .Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.Comment, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.Comment>,
                              IIncludableQueryable<DAL.Entities.Streetcode.Comment, object>>>()))
            .Callback<Expression<Func<DAL.Entities.Streetcode.Comment, bool>>, Func<IQueryable<DAL.Entities.Streetcode.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.Comment, object>>>(
                (pred, _) => capturedPredicate = pred)
            .ReturnsAsync(allComments);

        _mapperMock.Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<DAL.Entities.Streetcode.Comment>>()))
                   .Returns(approvedRootDtos);

        var query = new GetCommentsByStreetcodeIdQuery(33); // ForModeration = false

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedPredicate.Should().NotBeNull();
        var shouldBeIncluded = capturedPredicate!.Compile();
        shouldBeIncluded(allComments.First(c => c.IsApproved))
            .Should().BeTrue();
        shouldBeIncluded(allComments.First(c => !c.IsApproved))
            .Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldNotReturnComments_FromAnotherStreetcode()
    {
        // Arrange
        var foreignComment = new DAL.Entities.Streetcode.Comment
        {
            Id = 10,
            StreetcodeId = 44,
            ParentCommentId = null,
            IsApproved = true
        };

        var (ownComments, rootDtos, _) = CreateCommentsWithReplies();
        var allComments = ownComments.Append(foreignComment).ToList();
        SetupRepositoryReturn(allComments);

        _mapperMock.Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<DAL.Entities.Streetcode.Comment>>()))
                   .Returns(rootDtos);

        var query = new GetCommentsByStreetcodeIdQuery(33);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Should().OnlyContain(dto => dto.StreetcodeId == 33);
    }

    private static (List<DAL.Entities.Streetcode.Comment> entities,
                    List<CommentDTO> approvedRootDtos,
                    List<CommentDTO> allRootDtos) CreateCommentsWithReplies(bool includeUnapproved = false)
    {
        var approvedRoot = new DAL.Entities.Streetcode.Comment
        {
            Id = 1,
            ParentCommentId = null,
            StreetcodeId = 33,
            IsApproved = true
        };

        var replyToApproved = new DAL.Entities.Streetcode.Comment
        {
            Id = 2,
            ParentCommentId = 1,
            StreetcodeId = 33,
            IsApproved = true
        };

        var unapprovedRoot = new DAL.Entities.Streetcode.Comment
        {
            Id = 3,
            ParentCommentId = null,
            StreetcodeId = 33,
            IsApproved = false
        };

        var comments = new List<DAL.Entities.Streetcode.Comment> { approvedRoot, replyToApproved };
        if (includeUnapproved)
        {
            comments.Add(unapprovedRoot);
        }

        var approvedRootDtos = new List<CommentDTO>
        {
            new CommentDTO { Id = approvedRoot.Id, StreetcodeId = approvedRoot.StreetcodeId }
        };

        var allRootDtos = approvedRootDtos
            .Concat(includeUnapproved
                ? new[] { new CommentDTO { Id = unapprovedRoot.Id, StreetcodeId = unapprovedRoot.StreetcodeId } }
                : Array.Empty<CommentDTO>())
            .ToList();

        return (comments, approvedRootDtos, allRootDtos);
    }

    private void SetupRepositoryReturn(IEnumerable<DAL.Entities.Streetcode.Comment> entities)
    {
        _repositoryMock
            .Setup(r => r.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.Comment, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.Comment>,
                              IIncludableQueryable<DAL.Entities.Streetcode.Comment, object>>>()))
            .ReturnsAsync(entities);
    }

    private void SetupMapperReturn(List<CommentDTO> dtos)
    {
        _mapperMock
            .Setup(m => m.Map<List<CommentDTO>>(It.IsAny<List<DAL.Entities.Streetcode.Comment>>()))
            .Returns(dtos);
    }

    private void VerifyRepositoryCalledOnce()
        => _repositoryMock.Verify(
                r => r.CommentRepository.GetAllAsync(
                    It.IsAny<Expression<Func<DAL.Entities.Streetcode.Comment, bool>>>(),
                    It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.Comment>,
                                  IIncludableQueryable<DAL.Entities.Streetcode.Comment, object>>>()),
                Times.Once);
}