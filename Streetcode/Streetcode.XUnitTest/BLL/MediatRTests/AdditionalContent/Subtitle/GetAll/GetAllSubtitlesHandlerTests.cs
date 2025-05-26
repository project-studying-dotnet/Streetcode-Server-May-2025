using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Subtitle.GetAll;

public class GetAllSubtitlesHandlerTests
{
    private readonly GetAllSubtitlesHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;
    
    public GetAllSubtitlesHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetAllSubtitlesHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_SubtitlesExists_ReturnsAllSubtitlesSuccessfully()
    {
        // Arrange
        var subtitle = GetAllSubtitles();
        var subtitleDtos = GetAllSubtitlesDTOs();
        _repositoryWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync(subtitle);
        _mapper.Setup(x => x.Map<IEnumerable<SubtitleDTO>>(subtitle))
            .Returns(subtitleDtos);
        
        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.SubtitleRepository.GetAllAsync(null, null), Times.Once);
        _mapper.Verify(x => x.Map<IEnumerable<SubtitleDTO>>(subtitle), Times.Once);
    }

    [Fact]
    public async Task Handle_SubtitlesNotExists_ReturnsErrorMessage()
    {
        // Arange
        var expectedErrorMessage = "Cannot find any subtitles";
        _repositoryWrapper.Setup(r => r.SubtitleRepository.GetAllAsync(null, null))
            .ReturnsAsync((IEnumerable<DAL.Entities.AdditionalContent.Subtitle>)null);
        
        // Act
        var result = await _handler.Handle(new GetAllSubtitlesQuery(), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        _repositoryWrapper.Verify(r => r.SubtitleRepository.GetAllAsync(null, null), Times.Once);
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _mapper.Verify(x => x.Map<IEnumerable<SubtitleDTO>>(
            It.IsAny<IEnumerable<DAL.Entities.AdditionalContent.Subtitle>>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<GetAllSubtitlesQuery>(), "Cannot find any subtitles"), Times.Once);
    }

    private IEnumerable<DAL.Entities.AdditionalContent.Subtitle> GetAllSubtitles()
    {
        return new List<DAL.Entities.AdditionalContent.Subtitle>
        {
            new DAL.Entities.AdditionalContent.Subtitle { Id = 1 },
            new DAL.Entities.AdditionalContent.Subtitle { Id = 2 },
        };
    }
    
    private IEnumerable<SubtitleDTO> GetAllSubtitlesDTOs()
    {
        return new List<SubtitleDTO>
        {
            new SubtitleDTO { Id = 1 },
            new SubtitleDTO { Id = 2 },
        };
    }
}