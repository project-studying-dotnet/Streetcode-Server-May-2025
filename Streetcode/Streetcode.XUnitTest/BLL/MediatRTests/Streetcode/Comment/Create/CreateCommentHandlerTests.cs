using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Comment;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode;
using Xunit;

using CommentEntity = Streetcode.DAL.Entities.Streetcode.Comment;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Comment.Create;

public class CreateCommentHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly CreateCommentHandler _handler;

    public CreateCommentHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _commentRepoMock = new Mock<ICommentRepository>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _repositoryWrapperMock.Setup(r => r.CommentRepository).Returns(_commentRepoMock.Object);
        _handler = new CreateCommentHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidComment_ReturnsSuccess()
    {
        // Arrange
        var commentEntity = new CommentEntity();
        var createdEntity = new CommentEntity();
        var dto = new CommentDTO();

        var request = new CreateCommentCommand(new CreateCommentDTO());

        _mapperMock.Setup(m => m.Map<CommentEntity>(It.IsAny<CreateCommentDTO>())).Returns(commentEntity);
        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<CommentEntity>())).ReturnsAsync(createdEntity);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<CommentDTO>(createdEntity)).Returns(dto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_MappingToDtoFails_ReturnsFailure()
    {
        // Arrange
        var commentEntity = new CommentEntity();
        var createdEntity = new CommentEntity();

        var request = new CreateCommentCommand(new CreateCommentDTO());

        _mapperMock.Setup(m => m.Map<CommentEntity>(It.IsAny<CreateCommentDTO>())).Returns(commentEntity);
        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<CommentEntity>())).ReturnsAsync(createdEntity);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<CommentDTO>(createdEntity)).Returns((CommentDTO)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Cannot map entity"));
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ReturnsFailure()
    {
        // Arrange
        var commentEntity = new CommentEntity();
        var createdEntity = new CommentEntity();

        var request = new CreateCommentCommand(new CreateCommentDTO());

        _mapperMock.Setup(m => m.Map<CommentEntity>(It.IsAny<CreateCommentDTO>())).Returns(commentEntity);
        _commentRepoMock.Setup(r => r.CreateAsync(It.IsAny<CommentEntity>())).ReturnsAsync(createdEntity);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Cannot save changes"));
    }

    [Fact]
    public async Task Handle_MappingFails_ReturnsFailure()
    {
        // Arrange
        var request = new CreateCommentCommand(new CreateCommentDTO());
        _mapperMock.Setup(m => m.Map<CommentEntity>(It.IsAny<CreateCommentDTO>())).Returns((CommentEntity)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("Cannot create new comment"));
    }

}
