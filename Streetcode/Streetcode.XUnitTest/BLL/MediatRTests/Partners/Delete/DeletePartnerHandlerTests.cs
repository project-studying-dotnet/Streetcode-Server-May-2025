using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.BLL.MediatR.Partners.Delete;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;


namespace Streetcode.XUnitTest.BLL.MediatRTests.Partners.Delete
{
    public class DeletePartnerHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly DeletePartnerHandler _handler;

        public DeletePartnerHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new DeletePartnerHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeletePartner_WhenPartnerExistsAndNoException()
        {
            // Arrange
            var partnerId = 1;
            var partnerEntity = new PartnerEntity { Id = partnerId };
            var partnerDto = new PartnerDTO { Id = partnerId };

            _repositoryWrapperMock
               .Setup(r => r.PartnersRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<PartnerEntity, bool>>>(),
               null))
               .ReturnsAsync(partnerEntity);

            _mapperMock
                .Setup(m => m.Map<PartnerDTO>(partnerEntity))
                .Returns(partnerDto);

            var query = new DeletePartnerQuery(partnerId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(partnerDto, result.Value);
            _repositoryWrapperMock.Verify(r => r.PartnersRepository.Delete(partnerEntity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenPartnerDoesNotExist()
        {
            // Arrange
            var partnerId = 1;

            _repositoryWrapperMock
                .Setup(r => r.PartnersRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<PartnerEntity, bool>>>(), null))
                .ReturnsAsync((PartnerEntity)null);

            var query = new DeletePartnerQuery(partnerId);
            var expectedMessage = "No partner with such id";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
            _repositoryWrapperMock.Verify(r => r.PartnersRepository.Delete(It.IsAny<PartnerEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            var partnerId = 1;
            var partnerEntity = new PartnerEntity { Id = partnerId };
            var exceptionMessage = "DB error";

            _repositoryWrapperMock
                .Setup(r => r.PartnersRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<PartnerEntity, bool>>>(), null))
                .ReturnsAsync(partnerEntity);

            _repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new Exception(exceptionMessage));

            var query = new DeletePartnerQuery(partnerId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(exceptionMessage, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, exceptionMessage), Times.Once);
            _repositoryWrapperMock.Verify(r => r.PartnersRepository.Delete(partnerEntity), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }


    }
}
