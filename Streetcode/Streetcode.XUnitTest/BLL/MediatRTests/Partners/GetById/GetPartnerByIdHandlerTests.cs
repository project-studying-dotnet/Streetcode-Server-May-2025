using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetAll;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Partners.GetById
{
    public class GetPartnerByIdHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly GetPartnerByIdHandler _handler;

        public GetPartnerByIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new GetPartnerByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnPartner_WhenPartnerExists()
        {
            // Arrange
            var partner = new PartnerEntity { Id = 1 };
            var partnerDto = new PartnerDTO { Id = 1 };

            _repositoryWrapperMock
                .Setup(r => r.PartnersRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Partner, bool>>>(),
                    It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
                .ReturnsAsync(partner);

            _mapperMock
                .Setup(m => m.Map<PartnerDTO>(
                    It.IsAny<PartnerEntity>()))
                .Returns(partnerDto);

            var query = new GetPartnerByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(partnerDto, result.Value);
            _mapperMock.Verify(m => m.Map<PartnerDTO>(It.IsAny<PartnerEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenPartnerIsNull()
        {
            // Arrange
            _repositoryWrapperMock
                .Setup(r => r.PartnersRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<Partner, bool>>>(),
                    It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))
               .ReturnsAsync((PartnerEntity)null);

            var query = new GetPartnerByIdQuery(1);
            var expectedMessage = $"Cannot find a partner with corresponding id: {query.Id}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
            _mapperMock.Verify(m => m.Map<PartnerDTO>(It.IsAny<PartnerEntity>()), Times.Never);
        }
    }
}
