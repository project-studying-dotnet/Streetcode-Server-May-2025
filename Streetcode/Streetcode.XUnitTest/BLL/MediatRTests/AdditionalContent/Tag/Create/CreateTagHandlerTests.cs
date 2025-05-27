using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Tag.Create;

public class CreateTagHandlerTests
{
    private readonly CreateTagHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public CreateTagHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new CreateTagHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_TagCreatedSuccessfully_ReturnsCreatedTag()
    {
        // Arrange
        var createTagDto = new CreateTagDTO { Title = "Title" };
        var tag = new DAL.Entities.AdditionalContent.Tag { Title = createTagDto.Title };
        var tagDto = new TagDTO { Id = 1, Title = tag.Title };
        _repositoryWrapper.Setup(r => r.TagRepository.CreateAsync(
            It.IsAny<DAL.Entities.AdditionalContent.Tag>())).ReturnsAsync(tag);
        _repositoryWrapper.Setup(x => x.SaveChangesAsync());
        _mapper.Setup(m => m.Map<TagDTO>(tag)).Returns(tagDto);

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.TagRepository.CreateAsync(
            It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
        _repositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mapper.Verify(m => m.Map<TagDTO>(tag), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateFailed_ReturnsErrorMessage()
    {
        // Arrange
        var exceptionMessage = "Database error";
        var createTagDto = new CreateTagDTO { Title = "Title" };
        var tag = new DAL.Entities.AdditionalContent.Tag { Title = createTagDto.Title };
        _repositoryWrapper.Setup(r => r.TagRepository.CreateAsync(
            It.IsAny<DAL.Entities.AdditionalContent.Tag>())).ReturnsAsync(tag);
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _handler.Handle(new CreateTagQuery(createTagDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Message.Should().Contain("Database error");
        _repositoryWrapper.Verify(r => r.TagRepository.CreateAsync(
            It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
        _repositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mapper.Verify(m => m.Map<TagDTO>(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<object>(), It.Is<string>(msg => msg.Contains(exceptionMessage))), Times.Once);
    }
}