using System.Linq.Expressions;
using Ardalis.Specification;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Partners.GetAll;

public class GetAllPartnersHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly GetAllPartnersHandler _handler;

    public GetAllPartnersHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new GetAllPartnersHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPartners_WhenPartnersExist()
    {
        // Arrange
        var partners = new List<PartnerEntity> { new PartnerEntity { Id = 1 }, new PartnerEntity { Id = 2 } };
        var partnersDto = new List<PartnerDTO> { new PartnerDTO { Id = 1 }, new PartnerDTO { Id = 2 } };

        _repositoryWrapperMock
            .Setup(r => r.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
            .ReturnsAsync(partners);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<PartnerDTO>>(
                It.IsAny<IEnumerable<PartnerEntity>>()))
            .Returns(partnersDto);

        var query = new GetAllPartnersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(partnersDto, result.Value);
        _mapperMock.Verify(m => m.Map<IEnumerable<PartnerDTO>>(It.IsAny<IEnumerable<PartnerEntity>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPartnersListIsEmpty()
    {
        // Arrange
        var emptyList = new List<PartnerEntity>();
        var emptyDtos = new List<PartnerDTO>();

        _repositoryWrapperMock
            .Setup(r => r.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
            .ReturnsAsync(emptyList);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<PartnerDTO>>(emptyList))
            .Returns(emptyDtos);

        var query = new GetAllPartnersQuery();

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
       .Setup(r => r.PartnersRepository.ListAsync(
           It.IsAny<ISpecification<PartnerEntity>>(),
           It.IsAny<CancellationToken>()))
       .ReturnsAsync((List<PartnerEntity>)null);

        var query = new GetAllPartnersQuery();
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