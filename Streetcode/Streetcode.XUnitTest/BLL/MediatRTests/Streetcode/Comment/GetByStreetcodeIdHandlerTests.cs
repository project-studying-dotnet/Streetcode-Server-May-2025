using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment;

public class GetByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetCommentsByStreetcodeIdHandler _handler;

    public GetByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetCommentsByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnComments_WhenCommentsExist()
    {
        // Arrange
        int streetcodeId = 1;
        var comments = new List<Comment>
        {
            new Comment { Id = 1, StreetcodeId = streetcodeId, Text = "Test 1", Author = "A", CreatedAt = System.DateTime.UtcNow },
            new Comment { Id = 2, StreetcodeId = streetcodeId, Text = "Test 2", Author = "B", CreatedAt = System.DateTime.UtcNow }
        };
        var commentDtos = comments.Select(c => new CommentDto { Id = c.Id, StreetcodeId = c.StreetcodeId, Text = c.Text, Author = c.Author, CreatedAt = c.CreatedAt }).ToList();

        _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Comment, bool>>>(),
            null, false)).ReturnsAsync(comments);
        _mapperMock.Setup(m => m.Map<IEnumerable<CommentDto>>(comments)).Returns(commentDtos);

        var query = new GetCommentsByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(commentDtos);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenNoCommentsExist()
    {
        // Arrange
        int streetcodeId = 99;
        _repositoryWrapperMock.Setup(r => r.CommentRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<System.Func<Comment, bool>>>(),
            null, false)).ReturnsAsync((List<Comment>?)null);

        var query = new GetCommentsByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
} 