using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Update;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Coordinate.Update;

public class UpdateCoordinateHandlerTests
{
    private readonly UpdateCoordinateHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;

    public UpdateCoordinateHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _handler = new UpdateCoordinateHandler(_repositoryWrapper.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_GivenValidCoordinate_ShouldUpdateCoordinateSuccessfully()
    {
        // Arrange
        var coordinateDto = GetStreetcodeCoordinateDTO();
        var coordinate = GetStreetcodeCoordinate();
        _mapper.Setup(x => x.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(coordinate);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.Update(coordinate));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mapper.Verify(x => x.Map<StreetcodeCoordinate>(coordinateDto), Times.Once);
        _repositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.Update(coordinate), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidCoordinate_ShouldReturnError()
    {
        // Arange
        var expectedErrorMessage = "Cannot convert null to streetcodeCoordinate";
        var coordinateDto = new StreetcodeCoordinateDTO();
        _mapper.Setup(x => x.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns((StreetcodeCoordinate)null);

        // Act
        var result = await _handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _repositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.Update(
            It.IsAny<StreetcodeCoordinate>()), Times.Never);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFailed_ShouldReturnError()
    {
        // Arrange
        var expectedErrorMessage = "Failed to update a streetcodeCoordinate";
        var coordinateDto = GetStreetcodeCoordinateDTO();
        var coordinate = GetStreetcodeCoordinate();
        _mapper.Setup(x => x.Map<StreetcodeCoordinate>(It.IsAny<StreetcodeCoordinateDTO>()))
            .Returns(coordinate);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.Update(coordinate));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new UpdateCoordinateCommand(coordinateDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
    }

    private StreetcodeCoordinate GetStreetcodeCoordinate()
    {
        return new StreetcodeCoordinate
        {
            StreetcodeId = 1,
        };
    }

    private StreetcodeCoordinateDTO GetStreetcodeCoordinateDTO()
    {
        return new StreetcodeCoordinateDTO
        {
            StreetcodeId = 1,
        };
    }
}