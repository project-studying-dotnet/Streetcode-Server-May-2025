using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Subtitle.GetByStreetcodeId;

public class GetSubtitlesByStreetcodeIdHandlerTests
{
    private readonly GetSubtitlesByStreetcodeIdHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;

    public GetSubtitlesByStreetcodeIdHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _handler = new GetSubtitlesByStreetcodeIdHandler(_repositoryWrapper.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_GivenStreetcodeIdExist_ReturnsSubtitle()
    {
        // Arrange
        var streetcodeId = 1;
        var subtitle = GetSubtitle();
        var subtitleDto = GetSubtitleDto();
        _repositoryWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null))
            .ReturnsAsync(subtitle);
        _mapper.Setup(m => m.Map<SubtitleDTO>(subtitle)).Returns(subtitleDto);

        // Act
        var result = await _handler.Handle(
            new GetSubtitlesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<SubtitleDTO>();
        _repositoryWrapper.Verify(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null), Times.Once);
        _mapper.Verify(m => m.Map<SubtitleDTO>(subtitle), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenStreetcodeIdDoesNotExist_ReturnsNullResult()
    {
        // Arrange
        var streetcodeId = -1;
        _repositoryWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Subtitle)null);
        _mapper.Setup(m => m.Map<SubtitleDTO>(
            It.IsAny<DAL.Entities.AdditionalContent.Subtitle>())).Returns((SubtitleDTO)null);

        // Act
        var result = await _handler.Handle(new GetSubtitlesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    private DAL.Entities.AdditionalContent.Subtitle GetSubtitle()
    {
        return new DAL.Entities.AdditionalContent.Subtitle
        {
            Id = 1,
        };
    }

    private SubtitleDTO GetSubtitleDto()
    {
        return new SubtitleDTO
        {
            Id = 1,
        };
    }
}