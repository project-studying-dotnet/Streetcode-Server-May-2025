using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
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


namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.RelatedTermTests.Delete
{
    public class DeleteRelatedTermHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IRelatedTermRepository> _mockRelatedTermRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;

        public DeleteRelatedTermHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockRelatedTermRepository = new Mock<IRelatedTermRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();

            _mockRepositoryWrapper.Setup(x => x.RelatedTermRepository).Returns(_mockRelatedTermRepository.Object);
        }

        private async Task<Result<RelatedTermDTO>> ArrangeAndActAsync(string wordToFind, RelatedTerm foundEntity = null, int saveChangesResult = 1, RelatedTermDTO mappedDtoResult = null)
        {
            var command = new DeleteRelatedTermCommand(wordToFind);

            _mockRelatedTermRepository.Setup(repo => repo.GetFirstOrDefaultAsync(
                It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                null))
                .ReturnsAsync(foundEntity);

            if (foundEntity != null)
            {
                _mockRelatedTermRepository.Setup(repo => repo.Delete(foundEntity));
                _mockRepositoryWrapper.Setup(x => x.SaveChangesAsync()).ReturnsAsync(saveChangesResult);
                _mockMapper.Setup(m => m.Map<RelatedTermDTO>(foundEntity)).Returns(mappedDtoResult);
            }

            var handler = new DeleteRelatedTermHandler(
                _mockRepositoryWrapper.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            return await handler.Handle(command, CancellationToken.None);
        }

        // --- Successful Deletion Tests ---

        [Fact]
        public async Task Handle_ExistingTerm_ShouldReturnSuccessResult()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingTerm_ShouldReturnCorrectDtoValue()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            result.Value.Should().BeEquivalentTo(mappedDto);
        }

        [Fact]
        public async Task Handle_ExistingTerm_ShouldCallGetFirstOrDefaultAsync()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingTerm_ShouldCallRelatedTermRepositoryDelete()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            _mockRelatedTermRepository.Verify(repo => repo.Delete(foundEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingTerm_ShouldCallRepositoryWrapperSaveChangesAsync()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingTerm_ShouldCallMapperToMapEntityToDto()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(foundEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingTerm_ShouldNotCallLoggerError()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

            _mockLogger.Verify(logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), It.IsAny<string>()), Times.Never);
        }

        // --- Term Not Found Tests ---

        [Fact]
        public async Task Handle_TermNotFound_ShouldReturnFailureResult()
        {
            var wordToFind = "NonExistingWord";

            var result = await ArrangeAndActAsync(wordToFind, null);

            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_TermNotFound_ShouldContainCorrectErrorMessage()
        {
            var wordToFind = "NonExistingWord";
            var expectedErrorMessage = $"Cannot find a related term: {wordToFind}";

            var result = await ArrangeAndActAsync(wordToFind, null);

            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        }

        [Fact]
        public async Task Handle_TermNotFound_ShouldCallGetFirstOrDefaultAsync()
        {
            var wordToFind = "NonExistingWord";

            await ArrangeAndActAsync(wordToFind, null);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
                Times.Once);
        }

        [Fact]
        public async Task Handle_TermNotFound_ShouldNotCallRelatedTermRepositoryDelete()
        {
            var wordToFind = "NonExistingWord";

            await ArrangeAndActAsync(wordToFind, null);

            _mockRelatedTermRepository.Verify(repo => repo.Delete(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TermNotFound_ShouldNotCallRepositoryWrapperSaveChangesAsync()
        {
            var wordToFind = "NonExistingWord";

            await ArrangeAndActAsync(wordToFind, null);

            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_TermNotFound_ShouldNotCallMapperToMapEntityToDto()
        {
            var wordToFind = "NonExistingWord";

            await ArrangeAndActAsync(wordToFind, null);

            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()), Times.Never);
        }

        [Fact]
        public async Task Handle_TermNotFound_ShouldCallLoggerError()
        {
            var wordToFind = "NonExistingWord";
            var expectedErrorMessage = $"Cannot find a related term: {wordToFind}";

            await ArrangeAndActAsync(wordToFind, null);

            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), expectedErrorMessage),
                Times.Once);
        }

        // --- Error Saving Changes Tests ---

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldReturnFailureResult()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            var result = await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldContainCorrectErrorMessage()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };
            var expectedErrorMessage = "Failed to delete a related term";

            var result = await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallGetFirstOrDefaultAsync()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRelatedTermRepositoryDelete()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            _mockRelatedTermRepository.Verify(repo => repo.Delete(foundEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRepositoryWrapperSaveChangesAsync()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallMapperToMapEntityToDto()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(foundEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallLoggerError()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };
            var expectedErrorMessage = "Failed to delete a related term";

            await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), expectedErrorMessage),
                Times.Once);
        }

        // --- Error Mapping Entity to DTO after successful save Tests ---

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldReturnFailureResult()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

            var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, null); // mappedDtoResult is null

            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldContainCorrectErrorMessage()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var expectedErrorMessage = "Failed to delete a related term";

            var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, null); // mappedDtoResult is null

            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallGetFirstOrDefaultAsync()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallRelatedTermRepositoryDelete()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

            _mockRelatedTermRepository.Verify(repo => repo.Delete(foundEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallRepositoryWrapperSaveChangesAsync()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

            _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallMapperToMapEntityToDto()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

            await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

            _mockMapper.Verify(m => m.Map<RelatedTermDTO>(foundEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallLoggerError()
        {
            var existingWord = "ExistingWord";
            var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
            var expectedErrorMessage = "Failed to delete a related term";

            await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), expectedErrorMessage),
                Times.Once);
        }
    }
}
