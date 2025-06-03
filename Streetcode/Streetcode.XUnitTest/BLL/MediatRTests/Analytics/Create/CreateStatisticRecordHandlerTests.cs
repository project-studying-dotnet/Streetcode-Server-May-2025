using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Analytics.Create;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Analytics.Create;

public class CreateStatisticRecordHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CreateStatisticRecordHandler _handler;

    public CreateStatisticRecordHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new CreateStatisticRecordHandler(
            _mockRepositoryWrapper.Object,
            _mockLogger.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_MapperThrowsException_ShouldPropagateException()
    {
        // Arrange
        var createDto = new StatisticRecordCreateDTO { QrId = 1, Count = 5 }; // Мінімально необхідний DTO
        var command = new CreateStatisticRecordCommand(createDto);
        var mappingException = new AutoMapperMappingException("Simulated mapping failure");

        _mockMapper.Setup(m => m.Map<StatisticRecord>(createDto))
            .Throws(mappingException);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AutoMapperMappingException>()
            .WithMessage(mappingException.Message);

        _mockRepositoryWrapper.Verify(r => r.StatisticRecordRepository.CreateAsync(It.IsAny<StatisticRecord>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CreateAsyncThrowsException_ShouldPropagateException()
    {
        // Arrange
        var createDto = new StatisticRecordCreateDTO { QrId = 1, Count = 5 };
        var command = new CreateStatisticRecordCommand(createDto);
        var statisticRecordEntity = new StatisticRecord(); // Об'єкт, що повертається мапером
        var dbException = new InvalidOperationException("Simulated DB error on create");

        _mockMapper.Setup(m => m.Map<StatisticRecord>(createDto))
            .Returns(statisticRecordEntity);

        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.CreateAsync(statisticRecordEntity))
            .ThrowsAsync(dbException);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(dbException.Message);

        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncThrowsException_ShouldPropagateException()
    {
        // Arrange
        var createDto = new StatisticRecordCreateDTO { QrId = 1, Count = 5 };
        var command = new CreateStatisticRecordCommand(createDto);
        var statisticRecordEntity = new StatisticRecord();

        _mockMapper.Setup(m => m.Map<StatisticRecord>(createDto))
            .Returns(statisticRecordEntity);

        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.CreateAsync(statisticRecordEntity))
            .ReturnsAsync(statisticRecordEntity); // CreateAsync успішний

        var dbUpdateException = new Microsoft.EntityFrameworkCore.DbUpdateException("Simulated DB error on save changes");
        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ThrowsAsync(dbUpdateException);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Якщо ви використовуєте EF Core, DbUpdateException є типовим винятком
        await act.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateException>()
            .WithMessage(dbUpdateException.Message);

        _mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Never); // Лог про успіх не має бути викликаний
    }
}