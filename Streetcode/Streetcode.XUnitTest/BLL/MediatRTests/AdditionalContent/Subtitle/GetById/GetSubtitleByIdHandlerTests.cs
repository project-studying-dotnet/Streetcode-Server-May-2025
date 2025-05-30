using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.GetById;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Subtitle.GetById;

public class GetSubtitleByIdHandlerTests
{
    private readonly GetSubtitleByIdHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public GetSubtitleByIdHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetSubtitleByIdHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_GivenValidSubtitleId_ShouldReturnSubtitleSuccessfully()
    {
        // Arrange
        var subtitleId = 1;
        var subtitle = GetSubtitle();
        var subtitleDto = GetSubtitleDto();
        _repositoryWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null))
            .ReturnsAsync(subtitle);
        _mapper.Setup(m => m.Map<SubtitleDTO>(subtitle)).Returns(subtitleDto);

        // Act
        var result = await _handler.Handle(new GetSubtitleByIdQuery(subtitleId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null), Times.Once);
        _mapper.Verify(m => m.Map<SubtitleDTO>(subtitle), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidSubtitleId_ShouldReturnErrorMessage()
    {
        // Arrange
        var subtitleId = -1;
        var expectedErrorMessage = $"Cannot find a subtitle with corresponding id: {subtitleId}";
        _repositoryWrapper.Setup(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.AdditionalContent.Subtitle)null);

        // Act
        var result = await _handler.Handle(new GetSubtitleByIdQuery(subtitleId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should()
            .Be($"Cannot find a subtitle with corresponding id: {subtitleId}");
        _repositoryWrapper.Verify(r => r.SubtitleRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Subtitle, bool>>>(), null), Times.Once);
        _mapper.Verify(x => x.Map<SubtitleDTO>(
            It.IsAny<DAL.Entities.AdditionalContent.Subtitle>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<GetSubtitleByIdQuery>(), 
            $"Cannot find a subtitle with corresponding id: {subtitleId}"), Times.Once);
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