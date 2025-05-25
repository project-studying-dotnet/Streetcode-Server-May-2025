using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Create;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Coordinate.Create;

public class CreateCoordinateHandlerTests
{
    private readonly CreateCoordinateHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly Mock<IMapper> _mapper;

    public CreateCoordinateHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _mapper = new Mock<IMapper>();
        _handler = new CreateCoordinateHandler(_repositoryWrapper.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ValidCoordinate_CreateCoordinateSuccessfully()
    {
        // Arrange
        var streetcodeCoordinateDTO = GetStreetcodeCoordinateDTO();
        var streetcodeCoordinate = GetStreetcodeCoordinate();
        _mapper.Setup(x => x.Map<StreetcodeCoordinate>(streetcodeCoordinateDTO))
            .Returns(streetcodeCoordinate);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.CreateAsync(streetcodeCoordinate));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateCoordinateCommand(streetcodeCoordinateDTO), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mapper.Verify(x => x.Map<StreetcodeCoordinate>(streetcodeCoordinateDTO), Times.Once);
        _repositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.CreateAsync(streetcodeCoordinate), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCoordinate_ShouldReturnError()
    {
        // Arrange
        var expectedErrorMessage = "Cannot convert null to streetcodeCoordinate";
        var streetcodeCoordinateDTO = new StreetcodeCoordinateDTO();
        _mapper.Setup(x => x.Map<StreetcodeCoordinate>(streetcodeCoordinateDTO))
            .Returns((StreetcodeCoordinate)null);

        // Act
        var result = await _handler.Handle(new CreateCoordinateCommand(streetcodeCoordinateDTO), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
    }

    [Fact]
    public async Task Handle_CreateFailed_ShouldReturnError()
    {
        // Arrange
        var expectedErrorMessage = "Failed to create a streetcodeCoordinate";
        var streetcodeCoordinateDTO = GetStreetcodeCoordinateDTO();
        var streetcodeCoordinate = GetStreetcodeCoordinate();
        _mapper.Setup(x => x.Map<StreetcodeCoordinate>(streetcodeCoordinateDTO))
            .Returns(streetcodeCoordinate);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.CreateAsync(streetcodeCoordinate));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new CreateCoordinateCommand(streetcodeCoordinateDTO), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _mapper.Verify(x => x.Map<StreetcodeCoordinate>(streetcodeCoordinateDTO), Times.Once);
        _repositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.CreateAsync(streetcodeCoordinate), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
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