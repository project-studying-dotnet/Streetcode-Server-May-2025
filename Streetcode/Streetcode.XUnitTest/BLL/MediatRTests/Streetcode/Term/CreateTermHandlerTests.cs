using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Term;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.Term.Create;

public class CreateTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILoggerService> _loggerMock = new();
    private readonly CreateTermHandler _handler;

    public CreateTermHandlerTests()
    {
        _handler = new CreateTermHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenValidRequest()
    {
        // Arrange
        var requestDto = GetValidDto();
        var entity = GetMappedEntity(requestDto);
        var expectedDto = GetExpectedDto(entity);

        _mapperMock.Setup(m => m.Map<Entity>(requestDto)).Returns(entity);
        _repositoryMock.Setup(r => r.TermRepository.CreateAsync(entity)).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<TermDTO>(entity)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(new CreateTermCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenMappingFails()
    {
        // Arrange
        var requestDto = GetValidDto();
        _mapperMock.Setup(m => m.Map<Entity>(requestDto)).Returns((Entity?)null);

        // Act
        var result = await _handler.Handle(new CreateTermCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Cannot map CreateTermRequest to entity.");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenSaveFails()
    {
        // Arrange
        var requestDto = GetValidDto();
        var entity = GetMappedEntity(requestDto);

        _mapperMock.Setup(m => m.Map<Entity>(requestDto)).Returns(entity);
        _repositoryMock.Setup(r => r.TermRepository.CreateAsync(entity)).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(new CreateTermCommand(requestDto), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Failed to save new Term.");
    }

    private static TermCreateDTO GetValidDto() => new()
    {
        Title = "Term Title",
        Description = "Some term description",
    };

    private static Entity GetMappedEntity(TermCreateDTO dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
    };

    private static TermDTO GetExpectedDto(Entity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description
    };
}