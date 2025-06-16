using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Streetcode.Comment;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetById;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Comment;

public class GetCommentByIdTest
{
    private readonly Mock<IRepositoryWrapper> _mockRepo;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;

    public GetCommentByIdTest()
    {
        _mockRepo = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccessResult()
    {
        // Arrange
        var comment = new Comment { Id = 1, Text = "Test comment" };
        var commentDto = new AdminCommentDTO { Id = 1, Text = "Test comment" };

        _mockRepo.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Func<Comment, bool>>(),
            It.IsAny<Func<IQueryable<Comment>, IQueryable<Comment>>>()))
            .ReturnsAsync(comment);

        _mockMapper.Setup(m => m.Map<AdminCommentDTO>(comment))
            .Returns(commentDto);

        var handler = new GetCommentByIdHandler(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
        var query = new GetCommentByIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(commentDto, result.Value);
    }

    [Fact]
    public async Task Handle_InvalidId_ReturnsFailureResult()
    {
        // Arrange
        _mockRepo.Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Func<Comment, bool>>(),
            It.IsAny<Func<IQueryable<Comment>, IQueryable<Comment>>>()))
            .ReturnsAsync((Comment)null);

        var handler = new GetCommentByIdHandler(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
        var query = new GetCommentByIdQuery(1);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
    }
} 