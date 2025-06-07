using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using EntFact = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Fact.Delete;

public class DeleteFactHandlerTests
{
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationService;
    private readonly Mock<ILoggerService> _logger;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapper;
    private readonly DeleteFactHandler _handler;

    public DeleteFactHandlerTests()
    {
        _logger = new Mock<ILoggerService>();
        _repositoryWrapper = new Mock<IRepositoryWrapper>();
        _cacheInvalidationService = new Mock<ICacheInvalidationService>();
        _handler = new DeleteFactHandler(_repositoryWrapper.Object, _logger.Object, _cacheInvalidationService.Object);
    }

    [Fact]
    public async Task Handler_ShouldDeleteFactSuccessfully_WhenImageExists()
    {
        // Arrange
        var testFact = GetFact();
        SetUpMockRepositoryGetFirstOrDefaultAsync(testFact);
        _repositoryWrapper.Setup(r => r.FactRepository.Delete(testFact));
        _repositoryWrapper.Setup(r => r.ImageRepository.Delete(testFact.Image));
        SetUpMockRepositorySaveChangesAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteFactCommand(testFact.Id), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(r => r.ImageRepository.Delete(testFact.Image), Times.Once);
        _repositoryWrapper.Verify(r => r.FactRepository.Delete(testFact), Times.Once);
        _repositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldDeleteFactSuccessfully_WhenImageDoesNotExist()
    {
        // Arrange
        var testFact = GetFactWithoutImage();
        SetUpMockRepositoryGetFirstOrDefaultAsync(testFact);
        _repositoryWrapper.Setup(r => r.FactRepository.Delete(testFact));
        _repositoryWrapper.Setup(r => r.ImageRepository.Delete(testFact.Image));
        SetUpMockRepositorySaveChangesAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteFactCommand(testFact.Id), CancellationToken.None);

        // Assert
        _repositoryWrapper.Verify(r => r.ImageRepository.Delete(testFact.Image), Times.Never);
        _repositoryWrapper.Verify(r => r.FactRepository.Delete(testFact), Times.Once);
        _repositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handler_ShouldReturnErrorMessage_IdIsIncorrect()
    {
        // Arrange
        var testFact = GetFact();
        string errorMsg = $"Couldn't find a fact with id: {testFact.Id}";
        SetUpMockRepositoryGetFirstOrDefaultAsync(null);

        // Act
        var result = await _handler.Handle(new DeleteFactCommand(testFact.Id), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMsg);
    }

    [Fact]
    public async Task Handler_ShouldReturnErrorMessage_DeletedFailure()
    {
        // Arrange
        var testFact = GetFact();
        string errorMsg = "Failed to delete a fact";
        SetUpMockRepositoryGetFirstOrDefaultAsync(testFact);
        _repositoryWrapper.Setup(r => r.ImageRepository.Delete(testFact.Image));
        _repositoryWrapper.Setup(r => r.FactRepository.Delete(testFact));
        SetUpMockRepositorySaveChangesAsync(0);

        // Act
        var result = await _handler.Handle(new DeleteFactCommand(testFact.Id), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMsg);
    }

    private EntFact GetFact()
    {
        return new EntFact()
        {
            Id = 1,
            Title = "Title",
            FactContent = "FactContent",
            ImageId = 1,
            Image = new Image
            {
                Id = 1,
                BlobName = "test.jpg",
                MimeType = "image/jpeg",
            },
            StreetcodeId = 1,
        };
    }

    private EntFact GetFactWithoutImage()
    {
        return new EntFact()
        {
            Id = 1,
            Title = "Title",
            FactContent = "FactContent",
            ImageId = 1,
            StreetcodeId = 1,
        };
    }

    private void SetUpMockRepositoryGetFirstOrDefaultAsync(EntFact fact)
    {
        _repositoryWrapper.Setup(x => x.FactRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<EntFact, bool>>>(), null))
            .ReturnsAsync(fact);
    }

    private void SetUpMockRepositorySaveChangesAsync(int number)
    {
        _repositoryWrapper.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(number);
    }
}