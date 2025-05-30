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
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.RelatedTermTests.Delete;

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


    [Fact]
    public async Task Handle_ExistingTerm_ShouldReturnSuccessResult()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingTerm_ShouldReturnCorrectDtoValue()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        result.Value.Should().BeEquivalentTo(mappedDto);
    }

    [Fact]
    public async Task Handle_ExistingTerm_ShouldCallGetFirstOrDefaultAsync()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        _mockRelatedTermRepository.Verify(
            repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingTerm_ShouldCallRelatedTermRepositoryDelete()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        _mockRelatedTermRepository.Verify(repo => repo.Delete(foundEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingTerm_ShouldCallRepositoryWrapperSaveChangesAsync()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingTerm_ShouldCallMapperToMapEntityToDto()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        _mockMapper.Verify(m => m.Map<RelatedTermDTO>(foundEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingTerm_ShouldNotCallLoggerError()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, mappedDto);

        // Assert
        _mockLogger.Verify(logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public async Task Handle_TermNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var wordToFind = "NonExistingWord";

        // Act
        var result = await ArrangeAndActAsync(wordToFind, null);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldContainCorrectErrorMessage()
    {
        // Arrange
        var wordToFind = "NonExistingWord";
        var expectedErrorMessage = $"Cannot find a related term: {wordToFind}";

        // Act
        var result = await ArrangeAndActAsync(wordToFind, null);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldCallGetFirstOrDefaultAsync()
    {
        // Arrange
        var wordToFind = "NonExistingWord";

        // Act
        await ArrangeAndActAsync(wordToFind, null);

        // Assert
        _mockRelatedTermRepository.Verify(
            repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldNotCallRelatedTermRepositoryDelete()
    {
        // Arrange
        var wordToFind = "NonExistingWord";

        // Act
        await ArrangeAndActAsync(wordToFind, null);

        // Assert
        _mockRelatedTermRepository.Verify(repo => repo.Delete(It.IsAny<RelatedTerm>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldNotCallRepositoryWrapperSaveChangesAsync()
    {
        // Arrange
        var wordToFind = "NonExistingWord";

        // Act
        await ArrangeAndActAsync(wordToFind, null);

        // Assert
        _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldNotCallMapperToMapEntityToDto()
    {
        // Arrange
        var wordToFind = "NonExistingWord";

        // Act
        await ArrangeAndActAsync(wordToFind, null);

        // Assert
        _mockMapper.Verify(m => m.Map<RelatedTermDTO>(It.IsAny<RelatedTerm>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldCallLoggerError()
    {
        // Arrange
        var wordToFind = "NonExistingWord";
        var expectedErrorMessage = $"Cannot find a related term: {wordToFind}";

        // Act
        await ArrangeAndActAsync(wordToFind, null);

        // Assert
        _mockLogger.Verify(
            logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), expectedErrorMessage),
            Times.Once);
    }


    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldReturnFailureResult()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        var result = await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldContainCorrectErrorMessage()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };
        var expectedErrorMessage = "Failed to delete a related term";

        // Act
        var result = await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallGetFirstOrDefaultAsync()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        _mockRelatedTermRepository.Verify(
            repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRelatedTermRepositoryDelete()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        _mockRelatedTermRepository.Verify(repo => repo.Delete(foundEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallRepositoryWrapperSaveChangesAsync()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallMapperToMapEntityToDto()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        _mockMapper.Verify(m => m.Map<RelatedTermDTO>(foundEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncReturnsZero_ShouldCallLoggerError()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var mappedDto = new RelatedTermDTO { Id = 1, Word = existingWord, TermId = 1 };
        var expectedErrorMessage = "Failed to delete a related term";

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 0, mappedDto);

        // Assert
        _mockLogger.Verify(
            logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), expectedErrorMessage),
            Times.Once);
    }


    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldReturnFailureResult()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, null); // mappedDtoResult is null

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldContainCorrectErrorMessage()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var expectedErrorMessage = "Failed to delete a related term";

        // Act
        var result = await ArrangeAndActAsync(existingWord, foundEntity, 1, null); // mappedDtoResult is null

        // Assert
        result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
    }

    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallGetFirstOrDefaultAsync()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

        // Assert
        _mockRelatedTermRepository.Verify(
            repo => repo.GetFirstOrDefaultAsync(It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(), null),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallRelatedTermRepositoryDelete()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

        // Assert
        _mockRelatedTermRepository.Verify(repo => repo.Delete(foundEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallRepositoryWrapperSaveChangesAsync()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

        // Assert
        _mockRepositoryWrapper.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallMapperToMapEntityToDto()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

        // Assert
        _mockMapper.Verify(m => m.Map<RelatedTermDTO>(foundEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_MapEntityToDtoReturnsNull_ShouldCallLoggerError()
    {
        // Arrange
        var existingWord = "ExistingWord";
        var foundEntity = new RelatedTerm { Id = 1, Word = existingWord, TermId = 1 };
        var expectedErrorMessage = "Failed to delete a related term";

        // Act
        await ArrangeAndActAsync(existingWord, foundEntity, 1, null);

        // Assert
        _mockLogger.Verify(
            logger => logger.LogError(It.IsAny<DeleteRelatedTermCommand>(), expectedErrorMessage),
            Times.Once);
    }
}
