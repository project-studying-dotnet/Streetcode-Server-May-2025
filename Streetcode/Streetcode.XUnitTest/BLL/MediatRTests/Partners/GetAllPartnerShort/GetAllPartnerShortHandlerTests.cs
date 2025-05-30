using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.MediatR.Partners.GetAllPartnerShort;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Partners.GetAllPartnerShort;

public class GetAllPartnerShortHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly GetAllPartnerShortHandler _handler;

    public GetAllPartnerShortHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new GetAllPartnerShortHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPartnersShort_WhenPartnersExist()
    {
        // Arrange
        var partners = new List<PartnerEntity> { new PartnerEntity { Id = 1, Description = "Test1" }, new PartnerEntity { Id = 2, Description = "Test2" } };
        var partnersShortDto = new List<PartnerShortDTO> { new PartnerShortDTO { Id = 1 }, new PartnerShortDTO { Id = 2 } };

        _repositoryWrapperMock
            .Setup(r => r.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
            .ReturnsAsync(partners);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<PartnerShortDTO>>(
                It.IsAny<IEnumerable<PartnerEntity>>()))
            .Returns(partnersShortDto);

        var query = new GetAllPartnersShortQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(partnersShortDto, result.Value);
        _mapperMock.Verify(m => m.Map<IEnumerable<PartnerShortDTO>>(It.IsAny<IEnumerable<PartnerEntity>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPartnersListIsEmpty()
    {
        // Arrange
        var emptyList = new List<PartnerEntity>();
        var emptyDtos = new List<PartnerShortDTO>();

        _repositoryWrapperMock
            .Setup(r => r.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
            .ReturnsAsync(emptyList);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<PartnerShortDTO>>(emptyList))
            .Returns(emptyDtos);

        var query = new GetAllPartnersShortQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPartnersListIsNull()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(r => r.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
            .ReturnsAsync((List<PartnerEntity>)null);

        var query = new GetAllPartnersShortQuery();
        var expectedMessage = "Cannot find any partners";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedMessage, result.Errors.First().Message);
        _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<PartnerDTO>>(It.IsAny<IEnumerable<PartnerEntity>>()), Times.Never);
    }
}