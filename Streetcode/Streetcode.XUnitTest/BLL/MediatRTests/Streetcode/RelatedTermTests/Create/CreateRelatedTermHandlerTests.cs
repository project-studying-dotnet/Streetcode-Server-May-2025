using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.RelatedTermTests.Create
{
    public class CreateRelatedTermHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IRelatedTermRepository> _mockRelatedTermRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        public CreateRelatedTermHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockRelatedTermRepository = new Mock<IRelatedTermRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();

            _mockRepositoryWrapper.Setup(x => x.RelatedTermRepository).Returns(_mockRelatedTermRepository.Object);

            _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

            _mockRelatedTermRepository.Setup(repo => repo.Create(It.IsAny<RelatedTerm>()))
                .Returns((RelatedTerm entity) => entity);

            _mockRelatedTermRepository.Setup(repo => repo.GetAllAsync(
                    It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                    null))
                .ReturnsAsync(new List<RelatedTerm>());
        }


        private async Task<Result<RelatedTermDTO>> ArrangeAndActAsync(RelatedTermDTO relatedTermDtoToCreate, RelatedTerm relatedTermEntityToCreate, RelatedTermDTO createdRelatedTermDtoResult)
        {
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            _mockMapper.Setup(m => m.Map<RelatedTerm>(It.IsAny<RelatedTermDTO>()))
                .Returns(relatedTermEntityToCreate);
            _mockMapper.Setup(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()))
                .Returns(createdRelatedTermDtoResult);

            var handler = new CreateRelatedTermHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            return await handler.Handle(command, CancellationToken.None);
        }

        private async Task<Result<RelatedTermDTO>> ArrangeAndActForFailureAsync(CreateRelatedTermCommand command, RelatedTerm relatedTermEntityToCreate = null, RelatedTerm relatedTermEntityCreated = null, RelatedTermDTO createdRelatedTermDtoResult = null, List<RelatedTerm> existingTermsList = null, int saveChangesResult = 1, RelatedTermDTO mapEntityToDtoResult = null)
        {
            if (command.RelatedTerm != null)
            {
                _mockMapper.Setup(m => m.Map<RelatedTerm>(It.IsAny<RelatedTermDTO>()))
                    .Returns(relatedTermEntityToCreate);
            }
            else
            {
                _mockMapper.Setup(m => m.Map<RelatedTerm>(null)).Returns((RelatedTerm)null);
            }


            if (existingTermsList != null)
            {
                _mockRelatedTermRepository.Setup(repo => repo.GetAllAsync(
                        It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                        null))
                    .ReturnsAsync(existingTermsList);
            }
            else
            {
                _mockRelatedTermRepository.Setup(repo => repo.GetAllAsync(
                      It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                      null))
                  .ReturnsAsync(new List<RelatedTerm>());
            }


            _mockRelatedTermRepository.Setup(repo => repo.Create(It.IsAny<RelatedTerm>()))
                .Returns((RelatedTerm entity) => relatedTermEntityCreated ?? entity);

            _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync()).ReturnsAsync(saveChangesResult);

            if (mapEntityToDtoResult != null || mapEntityToDtoResult == null && createdRelatedTermDtoResult != null)
            {
                _mockMapper.Setup(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()))
                    .Returns(mapEntityToDtoResult);
            }


            var handler = new CreateRelatedTermHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            return await handler.Handle(command, CancellationToken.None);
        }



        [Fact]
        public async Task Handle_ValidCommand_ShouldReturnSuccessResult()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ТестовеСлово", TermId = 5 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var createdRelatedTermDtoResult = new RelatedTermDTO { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            var result = await ArrangeAndActAsync(relatedTermDtoToCreate, relatedTermEntityToCreate, createdRelatedTermDtoResult);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldReturnCorrectDtoValue()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ТестовеСлово", TermId = 5 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var createdRelatedTermDtoResult = new RelatedTermDTO { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act

            var result = await ArrangeAndActAsync(relatedTermDtoToCreate, relatedTermEntityToCreate, createdRelatedTermDtoResult);
            
            // Assert
            result.Value.Should().BeEquivalentTo(createdRelatedTermDtoResult);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCallMapperToMapDtoToEntity()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ТестовеСлово", TermId = 5 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var createdRelatedTermDtoResult = new RelatedTermDTO { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActAsync(relatedTermDtoToCreate, relatedTermEntityToCreate, createdRelatedTermDtoResult);

            _mockMapper.Verify(m => m.Map<RelatedTerm>(relatedTermDtoToCreate), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ТестовеСлово", TermId = 5 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var createdRelatedTermDtoResult = new RelatedTermDTO { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActAsync(relatedTermDtoToCreate, relatedTermEntityToCreate, createdRelatedTermDtoResult);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null), Times.Once);
        }

       
        [Fact]
        public async Task Handle_ValidCommand_ShouldCallRepositoryWrapperSaveChangesAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ТестовеСлово", TermId = 5 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var createdRelatedTermDtoResult = new RelatedTermDTO { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActAsync(relatedTermDtoToCreate, relatedTermEntityToCreate, createdRelatedTermDtoResult);

            // Assert
            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task Handle_ValidCommand_ShouldNotCallLoggerError()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ТестовеСлово", TermId = 5 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var createdRelatedTermDtoResult = new RelatedTermDTO { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActAsync(relatedTermDtoToCreate, relatedTermEntityToCreate, createdRelatedTermDtoResult);

            // Assert
            _mockLogger.Verify(logger => logger.LogError(It.IsAny<CreateRelatedTermCommand>(), It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            var result = await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null); // mapEntityToDtoResult: null для цього сценарію

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldContainCorrectErrorMessage()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            var result = await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            result.Errors.Should().ContainSingle(e => e.Message == "Cannot create new related word for a term!");
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldCallMapperToMapNullDtoToEntity()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            _mockMapper.Setup(m => m.Map<RelatedTerm>(null)).Returns((RelatedTerm)null);

            // Act
            await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTerm>(null), Times.Once);
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldNotCallRelatedTermRepositoryGetAllAsync()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            _mockRelatedTermRepository.Verify(repo => repo.GetAllAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null), Times.Never);
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldNotCallRelatedTermRepositoryCreate()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            _mockRelatedTermRepository.Verify(repo => repo.Create(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldNotCallRepositoryWrapperSaveChangesAsync()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldNotCallMapperToMapEntityToDto()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CommandWithNullRelatedTermDTO_ShouldCallLoggerError()
        {
            // Arrange
            var command = new CreateRelatedTermCommand(null);

            // Act
            await ArrangeAndActForFailureAsync(command, mapEntityToDtoResult: null);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<CreateRelatedTermCommand>(), "Cannot create new related word for a term!"),
                Times.Once);
        }


        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldReturnFailureResult()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            var result = await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldContainCorrectErrorMessage()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            
            // Act
            var result = await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            result.Errors.Should().ContainSingle(e => e.Message == "Слово з цим визначенням уже існує");
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldCallMapperToMapDtoToEntity()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTerm>(relatedTermDtoToCreate), Times.Once);
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null), Times.Once);
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldNotCallRelatedTermRepositoryCreate()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            _mockRelatedTermRepository.Verify(repo => repo.Create(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldNotCallRepositoryWrapperSaveChangesAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldNotCallMapperToMapEntityToDto()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RelatedTermAlreadyExists_ShouldCallLoggerError()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "ExistingWord", TermId = 1 };
            var relatedTermEntityExisting = new RelatedTerm { Id = 1, Word = "ExistingWord", TermId = 1 };
            var existingTermsList = new List<RelatedTerm> { relatedTermEntityExisting };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, existingTermsList: existingTermsList);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<CreateRelatedTermCommand>(), "Слово з цим визначенням уже існує"),
                Times.Once);
        }



        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldReturnFailureResult()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            var result = await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldContainCorrectErrorMessage()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            var result = await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            result.Errors.Should().ContainSingle(e => e.Message == "Cannot save changes in the database after related word creation!");
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallMapperToMapDtoToEntity()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTerm>(relatedTermDtoToCreate), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null), Times.Once);
        }


        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRepositoryWrapperSaveChangesAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldNotCallMapperToMapEntityToDto()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallLoggerError()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<CreateRelatedTermCommand>(), "Cannot save changes in the database after related word creation!"),
                Times.Once);
        }


        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldReturnFailureResult()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToMap", TermId = 3 };
            var relatedTermEntityCreated = new RelatedTerm { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            var result = await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityCreated, relatedTermEntityCreated: relatedTermEntityCreated, saveChangesResult: 1, mapEntityToDtoResult: null);

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldContainCorrectErrorMessage()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToMap", TermId = 3 };
            var relatedTermEntityCreated = new RelatedTerm { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            var result = await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityCreated, relatedTermEntityCreated: relatedTermEntityCreated, saveChangesResult: 1, mapEntityToDtoResult: null);

            // Assert
            result.Errors.Should().ContainSingle(e => e.Message == "Cannot map entity!");
        }


        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallMapperToMapDtoToEntity()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToMap", TermId = 3 };
            var relatedTermEntityCreated = new RelatedTerm { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityCreated, relatedTermEntityCreated: relatedTermEntityCreated, saveChangesResult: 1, mapEntityToDtoResult: null);

            // Assert
            _mockMapper.Verify(m => m.Map<RelatedTerm>(relatedTermDtoToCreate), Times.Once);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToMap", TermId = 3 };
            var relatedTermEntityCreated = new RelatedTerm { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityCreated, relatedTermEntityCreated: relatedTermEntityCreated, saveChangesResult: 1, mapEntityToDtoResult: null);

            // Assert
            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null), Times.Once);
        }


        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallRepositoryWrapperSaveChangesAsync()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToMap", TermId = 3 };
            var relatedTermEntityCreated = new RelatedTerm { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityCreated, relatedTermEntityCreated: relatedTermEntityCreated, saveChangesResult: 1, mapEntityToDtoResult: null);

            // Assert
            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRelatedTermRepositoryCreateAsync() // Renamed test for clarity
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToSave", TermId = 2 };
            var relatedTermEntityToCreate = new RelatedTerm { Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityToCreate, relatedTermEntityCreated: relatedTermEntityToCreate, saveChangesResult: 0);

            // Assert
            _mockRelatedTermRepository.Verify(repo => repo.CreateAsync(relatedTermEntityToCreate), Times.Once);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallLoggerError()
        {
            // Arrange
            var relatedTermDtoToCreate = new RelatedTermDTO { Id = 0, Word = "WordToMap", TermId = 3 };
            var relatedTermEntityCreated = new RelatedTerm { Id = 1, Word = relatedTermDtoToCreate.Word, TermId = relatedTermDtoToCreate.TermId };
            var command = new CreateRelatedTermCommand(relatedTermDtoToCreate);

            // Act
            await ArrangeAndActForFailureAsync(command, relatedTermEntityToCreate: relatedTermEntityCreated, relatedTermEntityCreated: relatedTermEntityCreated, saveChangesResult: 1, mapEntityToDtoResult: null);

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<CreateRelatedTermCommand>(), "Cannot map entity!"),
                Times.Once);
        }
    }
}
