using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetTagByTitle;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Tag.GetTagByTitle;

public class GetTagByTitleHandlerTests
{
    private readonly GetTagByTitleHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public GetTagByTitleHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetTagByTitleHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_GivenTagWithValidTitle_ShouldReturnsTagSuccessfully()
    {
        // Arrange
        var tagTitle = "Title";
        var tag = new DAL.Entities.AdditionalContent.Tag { Id = 1, Title = "Title" };
        var tagDto = new TagDTO { Id = tag.Id, Title = tag.Title };
        _repositoryWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync(tag);
        _mapper.Setup(m => m.Map<TagDTO>(tag)).Returns(tagDto);

        // Act
        var result = await _handler.Handle(new GetTagByTitleQuery(tagTitle), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null), Times.Once);
        _mapper.Verify(m => m.Map<TagDTO>(tag), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenTagWithInvalidTitle_ShouldReturnErrorMessage()
    {
        // Arrange
        var tagTitle = "Title";
        var expectedErrorMessage = $"Cannot find any tag by the title: {tagTitle}";
        _repositoryWrapper.Setup(r => r.TagRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Tag)null);

        // Act
        var result = await _handler.Handle(new GetTagByTitleQuery(tagTitle), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should()
            .Be(expectedErrorMessage);
        _repositoryWrapper.Verify(r => r.TagRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null), Times.Once);
        _mapper.Verify(x => x.Map<TagDTO>(
            It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<GetTagByTitleQuery>(), 
            expectedErrorMessage), Times.Once);
    }
}