using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Create;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Partners.Create
{
    public class CreatePartnerHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly CreatePartnerHandler _handler;

        public CreatePartnerHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new CreatePartnerHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreatePartner_WhenDataIsValid()
        {
            // Arrange
            var partnerId = 1;
            var streetcodeIds = new List<int> { 1, 2 };

            var requestDto = new CreatePartnerDTO
            {
                Streetcodes = streetcodeIds.Select(id => new StreetcodeShortDTO { Id = id }).ToList()
            };

            var partnerEntity = new PartnerEntity { Id = partnerId, Streetcodes = new List<StreetcodeContent>() };
            var createdEntity = new PartnerEntity { Id = partnerId, Streetcodes = new List<StreetcodeContent>() };

            var streetcodesFromDb = streetcodeIds.Select(id => new StreetcodeContent { Id = id }).ToList();

            var expectedDto = new PartnerDTO { Id = 1 };

            _mapperMock.Setup(m => m.Map<PartnerEntity>(requestDto)).Returns(partnerEntity);
            _repositoryWrapperMock.Setup(r => r.PartnersRepository.CreateAsync(partnerEntity)).ReturnsAsync(createdEntity);
            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                     .ReturnsAsync(streetcodesFromDb);
            _mapperMock.Setup(m => m.Map<PartnerDTO>(createdEntity)).Returns(expectedDto);

            var query = new CreatePartnerQuery(requestDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDto, result.Value);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ShouldCreatePartner_WhenStreetcodesEmpty()
        {
            // Arrange
            var requestDto = new CreatePartnerDTO { Streetcodes = new List<StreetcodeShortDTO>() };

            var partnerEntity = new PartnerEntity { Id = 1, Streetcodes = new List<StreetcodeContent>() };
            var createdEntity = new PartnerEntity { Id = 1, Streetcodes = new List<StreetcodeContent>() };
            var expectedDto = new PartnerDTO { Id = 1 };

            _mapperMock.Setup(m => m.Map<PartnerEntity>(requestDto)).Returns(partnerEntity);
            _repositoryWrapperMock.Setup(r => r.PartnersRepository.CreateAsync(partnerEntity)).ReturnsAsync(createdEntity);
            _mapperMock.Setup(m => m.Map<PartnerDTO>(createdEntity)).Returns(expectedDto);

            var query = new CreatePartnerQuery(requestDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedDto, result.Value);
            _repositoryWrapperMock.Verify(r => r.StreetcodeRepository.GetAllAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null), Times.Never);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenExceptionThrown()
        {
            // Arrange
            var requestDto = new CreatePartnerDTO
            {
                Streetcodes = new List<StreetcodeShortDTO> { new StreetcodeShortDTO { Id = 1 } }
            };

            var partnerEntity = new PartnerEntity();

            _mapperMock.Setup(m => m.Map<PartnerEntity>(requestDto)).Returns(partnerEntity);
            _repositoryWrapperMock.Setup(r => r.PartnersRepository.CreateAsync(It.IsAny<Partner>()))
                     .ThrowsAsync(new Exception("Database error"));

            var query = new CreatePartnerQuery(requestDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Database error", result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(query, "Database error"), Times.Once);
        }

    }
}
