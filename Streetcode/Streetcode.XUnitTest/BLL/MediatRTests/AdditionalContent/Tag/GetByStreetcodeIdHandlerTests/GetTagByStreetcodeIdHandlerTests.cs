using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Tag.GetByStreetcodeIdHandlerTests;

public class GetTagByStreetcodeIdHandlerTests
{
    private readonly GetTagByStreetcodeIdHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public GetTagByStreetcodeIdHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetTagByStreetcodeIdHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_GivenValidStreetcodeId_ShouldReturnSuccessfully()
    {
        // Arrange
        var streetcodeId = 1;
        IEnumerable<StreetcodeTagIndex> tagIndexed = new List<StreetcodeTagIndex>
        {
            new StreetcodeTagIndex { StreetcodeId = streetcodeId, Index = 1 },
            new StreetcodeTagIndex { StreetcodeId = streetcodeId, Index = 2 },
        };
        _repositoryWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync(tagIndexed);
        IEnumerable<StreetcodeTagDTO> streetcodeTagDtos = new List<StreetcodeTagDTO>()
        {
            new StreetcodeTagDTO { Id = 1, Index = 2 },
            new StreetcodeTagDTO { Id = 2, Index = 2 },
        };
        var sortedTags = tagIndexed.OrderBy(t => t.Index).ToList();
        _mapper.Setup(m => m.Map<IEnumerable<StreetcodeTagDTO>>(sortedTags))
            .Returns(streetcodeTagDtos);

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.StreetcodeTagIndexRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()),
            Times.Once);
        _mapper.Verify(m => m.Map<IEnumerable<StreetcodeTagDTO>>(sortedTags), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidStreetcodeId_ShouldReturnError()
    {
        // Arrange
        var streetcodeId = 1;
        var expectedErrorMessage = $"Cannot find any tag by the streetcode id: {streetcodeId}";
        _repositoryWrapper.Setup(r => r.StreetcodeTagIndexRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
            .ReturnsAsync((IEnumerable<StreetcodeTagIndex>)null);

        // Act
        var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _repositoryWrapper.Verify(r => r.StreetcodeTagIndexRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()),
            Times.Once);
        _mapper.Verify(m => m.Map<IEnumerable<StreetcodeTagDTO>>(
            It.IsAny<IEnumerable<StreetcodeTagIndex>>()), Times.Never);
        _logger.Verify(l => l.LogError(
            It.IsAny<GetTagByStreetcodeIdQuery>(), 
            expectedErrorMessage), Times.Once);
    }
}