using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Delete;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.AdditionalContent.Coordinate.Delete;

public class DeleteCoordinateHandlerTests
{
    private readonly DeleteCoordinateHandler _handler;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;

    public DeleteCoordinateHandlerTests()
    {
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _handler = new DeleteCoordinateHandler(_repositoryWrapper.Object);
    }

    [Fact]
    public async Task Handle_GivenValidCoordinateId_ShouldDeleteCoordinateSuccessfully()
    {
        // Arrange
        var id = 1;
        var streetcodeCoordinate = GetStreetcodeCoordinate();
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(streetcodeCoordinate);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.Delete(streetcodeCoordinate));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteCoordinateCommand(id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryWrapper.Verify(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
        _repositoryWrapper.Verify(x => x.StreetcodeCoordinateRepository.Delete(streetcodeCoordinate), Times.Once);
        _repositoryWrapper.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_GivenInvalidCoordinateId_ShouldReturnError()
    {
        // Arrange
        var id = -1;
        var expectedErrorMessage = $"Cannot find a coordinate with corresponding categoryId: {id}";
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync((StreetcodeCoordinate)null);

        // Act
        var result = await _handler.Handle(new DeleteCoordinateCommand(id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
        _repositoryWrapper.Verify(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null), Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteFailed_ShouldReturnError()
    {
        // Arrage
        var expectedErrorMessage = "Failed to delete a coordinate";
        var id = 1;
        var streetcodeCoordinate = GetStreetcodeCoordinate();
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(streetcodeCoordinate);
        _repositoryWrapper.Setup(r => r.StreetcodeCoordinateRepository.Delete(streetcodeCoordinate));
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new DeleteCoordinateCommand(id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedErrorMessage);
    }

    private StreetcodeCoordinate GetStreetcodeCoordinate()
    {
        return new StreetcodeCoordinate
        {
            StreetcodeId = 1,
        };
    }
}