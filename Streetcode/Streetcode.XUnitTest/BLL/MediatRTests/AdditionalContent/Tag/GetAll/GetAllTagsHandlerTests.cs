using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Tag.GetAll;

public class GetAllTagsHandlerTests
{
    private readonly GetAllTagsHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public GetAllTagsHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetAllTagsHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_TagsExists_ReturnsAllTagsSuccessfully()
    {
        // Arrange
        IEnumerable<DAL.Entities.AdditionalContent.Tag> tags = new List<DAL.Entities.AdditionalContent.Tag>
        {
            new DAL.Entities.AdditionalContent.Tag { Title = "title1" },
            new DAL.Entities.AdditionalContent.Tag { Title = "title2" },
        };
        IEnumerable<TagDTO> tagDtos = new List<TagDTO>
        {
            new TagDTO { Title = "title1" },
            new TagDTO { Title = "title2" },
        };
        _repositoryWrapper.Setup(r => r.TagRepository.GetAllAsync(
            null, null)).ReturnsAsync(tags);
        _mapper.Setup(m => m.Map<System.Collections.Generic.IEnumerable<TagDTO>>(tags))
            .Returns(tagDtos);

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.TagRepository.GetAllAsync(null, null), Times.Once);
        _mapper.Verify(m => m.Map<System.Collections.Generic.IEnumerable<TagDTO>>(tags), Times.Once);
    }

    [Fact]
    public async Task Handle_TagsDoesNotExist_ReturnsErrorMessage()
    {
        // Arrange
        var expectedErrorMessage = "Cannot find any tags";
        _repositoryWrapper.Setup(r => r.TagRepository.GetAllAsync(
            null, null)).ReturnsAsync((IEnumerable<DAL.Entities.AdditionalContent.Tag>)null);

        // Act
        var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _mapper.Verify(m => m.Map<System.Collections.Generic.IEnumerable<TagDTO>>(
            It.IsAny<IEnumerable<DAL.Entities.AdditionalContent.Tag>>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<GetAllTagsQuery>(), 
            "Cannot find any tags"), Times.Once);
    }
}