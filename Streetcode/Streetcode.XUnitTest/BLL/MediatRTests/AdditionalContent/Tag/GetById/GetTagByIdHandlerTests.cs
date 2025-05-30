using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.GetById;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Tag.GetById;

public class GetTagByIdHandlerTests
{
    private readonly GetTagByIdHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public GetTagByIdHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetTagByIdHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_GivenValidId_ShouldReturnsTagSuccessfully()
    {
        // Arrange
        var tagId = 1;
        var tag = GetTag();
        var tagDto = GetTagDto();
        _repositoryWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync(tag);
        _mapper.Setup(m => m.Map<TagDTO>(tag)).Returns(tagDto);

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(tagId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null), Times.Once);
        _mapper.Verify(m => m.Map<TagDTO>(tag), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidId_ShouldReturnErrorMessage()
    {
        // Arrange
        var tagId = -1;
        var expectedErrorMessage = $"Cannot find a Tag with corresponding id: {tagId}";
        _repositoryWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag)null);

        // Act
        var result = await _handler.Handle(new GetTagByIdQuery(tagId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should()
            .Be(expectedErrorMessage);
        _repositoryWrapper.Verify(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null), Times.Once);
        _mapper.Verify(x => x.Map<TagDTO>(
            It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<GetTagByIdQuery>(), 
            expectedErrorMessage), Times.Once);
    }

    private DAL.Entities.AdditionalContent.Tag GetTag()
    {
        return new DAL.Entities.AdditionalContent.Tag()
        {
            Id = 1,
        };
    }

    private TagDTO GetTagDto()
    {
        return new TagDTO
        {
            Id = 1,
        };
    }
}