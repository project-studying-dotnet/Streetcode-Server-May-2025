using System.Linq.Expressions;
using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Fact.Create
{
    public class CreateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateFactHandler _handler;

        public CreateFactHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new CreateFactHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenDtoMapsToNullEntity()
        {
            // Arrange
            var requestDto = new FactUpdateCreateDTO();
            _mapperMock
                .Setup(m => m.Map<Entity>(requestDto))
                .Returns((Entity)null);

            // Act
            var result = await _handler.Handle(new CreateFactCommand(requestDto), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenStreetcodeIdIsMissing()
        {
            // Arrange
            var requestDto = new FactUpdateCreateDTO
            {
                StreetcodeId = 0,
                FactContent = "Some content"
            };
            var mappedEntity = new Entity
            {
                StreetcodeId = 0,
                FactContent = "Some content"
            };
            _mapperMock
                .Setup(m => m.Map<Entity>(requestDto))
                .Returns(mappedEntity);

            // Act
            var result = await _handler.Handle(new CreateFactCommand(requestDto), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenDuplicateFactExists()
        {
            // Arrange
            var requestDto = new FactUpdateCreateDTO
            {
                StreetcodeId = 5,
                FactContent = "Duplicate"
            };
            var mappedEntity = new Entity
            {
                StreetcodeId = 5,
                FactContent = "Duplicate"
            };
            _mapperMock
                .Setup(m => m.Map<Entity>(requestDto))
                .Returns(mappedEntity);

            _repositoryWrapperMock
                .Setup(r => r.FactRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Entity, bool>>>(), default))
                .ReturnsAsync(new Entity());

            // Act
            var result = await _handler.Handle(new CreateFactCommand(requestDto), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenNewFactIsSaved()
        {
            // Arrange
            var requestDto = new FactUpdateCreateDTO
            {
                StreetcodeId = 10,
                FactContent = "Unique"
            };
            var mappedEntity = new Entity
            {
                StreetcodeId = 10,
                FactContent = "Unique"
            };
            _mapperMock
                .Setup(m => m.Map<Entity>(requestDto))
                .Returns(mappedEntity);

            _repositoryWrapperMock
                .Setup(r => r.FactRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Entity, bool>>>(), default))
                .ReturnsAsync((Entity)null);

            _repositoryWrapperMock
                .Setup(r => r.FactRepository.CreateAsync(mappedEntity))
                .ReturnsAsync(mappedEntity);

            _repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mapperMock
                .Setup(m => m.Map<FactDTO>(mappedEntity))
                .Returns(new FactUpdateCreateDTO
                {
                    StreetcodeId = 10,
                    FactContent = "Unique"
                });

            // Act
            var result = await _handler.Handle(new CreateFactCommand(requestDto), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenSaveChangesReturnsZero()
        {
            // Arrange
            var requestDto = new FactUpdateCreateDTO
            {
                StreetcodeId = 20,
                FactContent = "WillFail"
            };
            var mappedEntity = new Entity
            {
                StreetcodeId = 20,
                FactContent = "WillFail"
            };
            _mapperMock
                .Setup(m => m.Map<Entity>(requestDto))
                .Returns(mappedEntity);

            _repositoryWrapperMock
                .Setup(r => r.FactRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Entity, bool>>>(), default))
                .ReturnsAsync((Entity)null);

            _repositoryWrapperMock
                .Setup(r => r.FactRepository.CreateAsync(mappedEntity))
                .ReturnsAsync(mappedEntity);

            _repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(new CreateFactCommand(requestDto), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
        }
    }
}
