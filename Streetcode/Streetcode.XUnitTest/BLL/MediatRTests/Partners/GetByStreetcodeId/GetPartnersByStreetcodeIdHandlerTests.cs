using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Partners.GetAll;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Partners.GetByStreetcodeId
{
    public class GetPartnersByStreetcodeIdHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly GetPartnersByStreetcodeIdHandler _handler;

        public GetPartnersByStreetcodeIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new GetPartnersByStreetcodeIdHandler(_mapperMock.Object, _repositoryWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnPartners_WhenPartnersExist()
        {
            // Arrange
            var streetcodeId = 1;

            var partner = new PartnerEntity { Id = 1, Streetcodes = new List<StreetcodeContent> { new StreetcodeContent { Id = streetcodeId } }};
            var partnerDto = new PartnerDTO { Id = 1 };

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    null))
                .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

            _repositoryWrapperMock
                .Setup(r => r.PartnersRepository.GetAllAsync(
                    It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
                .ReturnsAsync(new List<PartnerEntity> { partner });

            _mapperMock
               .Setup(m => m.Map<IEnumerable<PartnerDTO>>(It.IsAny<IEnumerable<PartnerEntity>>()))
               .Returns(new List<PartnerDTO> { partnerDto });

            var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var returnedPartner = Assert.Single(result.Value);
            Assert.Equal(partnerDto.Id, returnedPartner.Id);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenPartnersListIsEmpty()
        {
            // Arrange
            var streetcodeId = 1;

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    null))
                .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

            _repositoryWrapperMock
                .Setup(r => r.PartnersRepository.GetAllAsync(
                    It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
                .ReturnsAsync(new List<PartnerEntity>());

            _mapperMock
                .Setup(m => m.Map<IEnumerable<PartnerDTO>>(It.IsAny<IEnumerable<PartnerEntity>>()))
                .Returns(new List<PartnerDTO>());

            var query = new GetPartnersByStreetcodeIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenStreetcodeIsNotExist()
        {
            // Arrange
            var streetcodeId = 1;

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    null))
                .ReturnsAsync((StreetcodeContent)null);

            var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);
            var expectedMessage = $"Cannot find any partners with corresponding streetcode id: {query.StreetcodeId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenPartnersListIsNull()
        {
            // Arrange
            var streetcodeId = 1;

            _repositoryWrapperMock
                .Setup(r => r.StreetcodeRepository.GetSingleOrDefaultAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    null))
                .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

            _repositoryWrapperMock
               .Setup(r => r.PartnersRepository.GetAllAsync(
                   It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<PartnerEntity>, IIncludableQueryable<PartnerEntity, object>>>()))
                .ReturnsAsync((List<PartnerEntity>)null);

            var query = new GetPartnersByStreetcodeIdQuery(streetcodeId);
            var expectedMessage = $"Cannot find partners by a streetcode id: {query.StreetcodeId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        }
    }
}
