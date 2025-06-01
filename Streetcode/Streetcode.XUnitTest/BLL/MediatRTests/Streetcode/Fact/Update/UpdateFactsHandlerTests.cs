using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Fact.Update;

public class UpdateFactsHandlerTests
{
    private readonly UpdateFactsHandler _handler;
    private readonly Mock<IRepositoryWrapper> _mockRepoWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<ICacheInvalidationService> _mockCacheInvalidationService;

    public UpdateFactsHandlerTests()
    {
        _mockRepoWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLoggerService = new Mock<ILoggerService>();
        _mockCacheInvalidationService = new Mock<ICacheInvalidationService>();
        _handler = new UpdateFactsHandler(
            _mockLoggerService.Object, 
            _mockMapper.Object, 
            _mockRepoWrapper.Object,
            _mockCacheInvalidationService.Object);
    }

    [Fact]
    public async Task Handle_IdIsCorrect_ShouldUpdateSuccessfully()
    {
        // Arrange
        var factDto = GetFactDto();
        var fact = GetFact();
        _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Fact>(factDto)).Returns(fact);
        _mockRepoWrapper.Setup(r => r.FactRepository.Update(It.IsAny<DAL.Entities.Streetcode.TextContent.Fact>()));
        _mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<FactDTO>(fact))
            .Returns(factDto);

        // Act
        var result = await _handler.Handle(new UpdateFactsCommand(factDto), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<DAL.Entities.Streetcode.TextContent.Fact>(factDto), Times.Once);
        _mockRepoWrapper.Verify(r => r.FactRepository.Update(It.IsAny<DAL.Entities.Streetcode.TextContent.Fact>()), Times.Once);
        _mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockMapper.Verify(m => m.Map<FactDTO>(fact), Times.Once);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_IdIsIncorrect_ShouldReturnError()
    {
        // Arrange
        var errorMessage = "Cannot convert null to Fact";
        var factDto = GetFactDto();
        var fact = GetFact();
        _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Fact>(factDto))
            .Returns((DAL.Entities.Streetcode.TextContent.Fact)null);

        // Act
        var result = await _handler.Handle(new UpdateFactsCommand(factDto), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<DAL.Entities.Streetcode.TextContent.Fact>(factDto), Times.Once);
        _mockMapper.Verify(m => m.Map<FactDTO>(fact), Times.Never);
        _mockRepoWrapper.Verify(r => r.FactRepository.Update(It.IsAny<DAL.Entities.Streetcode.TextContent.Fact>()), Times.Never);
        _mockRepoWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockLoggerService.Verify(l => l.LogError(
            It.IsAny<UpdateFactsCommand>(), It.Is<string>(s => s.Contains(errorMessage))), Times.Once);
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdateFails_ShouldReturnError()
    {
        // Arrange
        var errorMessage = "Failed to update facts";
        var factDto = GetFactDto();
        var fact = GetFact();
        _mockMapper.Setup(m => m.Map<DAL.Entities.Streetcode.TextContent.Fact>(factDto)).Returns(fact);
        _mockRepoWrapper.Setup(r => r.FactRepository.Update(It.IsAny<DAL.Entities.Streetcode.TextContent.Fact>()));
        _mockRepoWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new UpdateFactsCommand(factDto), CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<FactDTO>(fact), Times.Never);
        _mockLoggerService.Verify(l => l.LogError(
            It.IsAny<UpdateFactsCommand>(), It.Is<string>(s => s.Contains(errorMessage))), Times.Once);
        result.IsFailed.Should().BeTrue();
    }

    private DAL.Entities.Streetcode.TextContent.Fact GetFact()
    {
        return new DAL.Entities.Streetcode.TextContent.Fact
        {
            Id = 1,
            Title = "Test",
            FactContent = "Test",
            ImageId = 1,
            StreetcodeId = 1,
        };
    }

    private FactDTO GetFactDto()
    {
        return new FactDTO
        {
            Id = 1,
            Title = "Test",
            FactContent = "Test",
            ImageId = 1,
            StreetcodeId = 1,
        };
    }
}