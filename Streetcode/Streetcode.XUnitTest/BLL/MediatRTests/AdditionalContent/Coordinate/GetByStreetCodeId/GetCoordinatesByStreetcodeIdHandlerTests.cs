using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Coordinate.GetByStreetCodeId;

public class GetCoordinatesByStreetcodeIdHandlerTests
{
    private readonly GetCoordinatesByStreetcodeIdHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;
    private readonly Mock<ILoggerService> _logger;

    public GetCoordinatesByStreetcodeIdHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _logger = new Mock<ILoggerService>();
        _handler = new GetCoordinatesByStreetcodeIdHandler(_repositoryWrapper.Object, _mapper.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_GivenValidStreetcodeId_ReturnsCoordinateSuccessfully()
    {
        // Arrange
        var streetcodeId = 1;
        var streetcode = new StreetcodeContent { Id = streetcodeId };
        var coordinates = GetCoordinates();
        _repositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(streetcode);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(coordinates);
        _mapper.Setup(x => x.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates))
            .Returns(getCoordinatesDtos());

        // Act
        var result = await _handler.Handle(new GetCoordinatesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(c => c.StreetcodeId).Should().BeEquivalentTo(new[] { 1, 2 });
        _repositoryWrapper.Verify(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null), Times.Once);
        _repositoryWrapper.Verify(r => r.StreetcodeCoordinateRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _mapper.Verify(x => x.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidStreetcodeId_ShouldReturnError()
    {
        // Arrange
        var streetcodeId = -1;
        var expectedErrorMessage =
            $"Cannot find a coordinates by a streetcode id: {streetcodeId}, because such streetcode doesn`t exist";
        _repositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent)null);

        // Act
        var result = await _handler.Handle(new GetCoordinatesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _repositoryWrapper.Verify(r => r.StreetcodeCoordinateRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null), Times.Never);
        _mapper.Verify(x => x.Map<IEnumerable<StreetcodeCoordinateDTO>>(It.IsAny<StreetcodeCoordinate>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CoordinatesDoesNotExist_ShouldReturnError()
    {
        // Arrange
        var streetcodeId = 1;
        var expectedErrorMessage = $"Cannot find a coordinates by a streetcode id: {streetcodeId}";
        var streetcode = new StreetcodeContent { Id = streetcodeId };
        _repositoryWrapper.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(streetcode);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync((IEnumerable<StreetcodeCoordinate>)null);

        // Act
        var result = await _handler.Handle(new GetCoordinatesByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _mapper.Verify(x => x.Map<IEnumerable<StreetcodeCoordinateDTO>>(It.IsAny<StreetcodeCoordinate>()), Times.Never);
        _logger.Verify(
            l => l.LogError(It.IsAny<GetCoordinatesByStreetcodeIdQuery>(), $"Cannot find a coordinates by a streetcode id: {streetcodeId}"),
            Times.Once);
    }

    private IEnumerable<StreetcodeCoordinate> GetCoordinates()
    {
        return new List<StreetcodeCoordinate>
        {
            new StreetcodeCoordinate { Id = 1, Streetcode = new StreetcodeContent { Id = 1 } },
            new StreetcodeCoordinate { Id = 2, Streetcode = new StreetcodeContent { Id = 2 } },
        };
    }

    private IEnumerable<StreetcodeCoordinateDTO> getCoordinatesDtos()
    {
        return new List<StreetcodeCoordinateDTO>
        {
            new StreetcodeCoordinateDTO { StreetcodeId = 1 },
            new StreetcodeCoordinateDTO { StreetcodeId = 2 },
        };
    }
}