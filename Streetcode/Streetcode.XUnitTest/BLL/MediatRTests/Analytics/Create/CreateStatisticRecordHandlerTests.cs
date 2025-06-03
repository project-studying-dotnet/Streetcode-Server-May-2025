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
    public async Task Handle_ValidRequest_ShouldCreateRecordAndReturnSuccess()
    {
        // Arrange
        var createDto = new StatisticRecordCreateDTO
        {
            QrId = 1,
            Count = 10,
            Address = "127.0.0.1",
            StreetcodeId = 1,
            StreetcodeCoordinateId = 101
        };
        var command = new CreateStatisticRecordCommand(createDto);

        var statisticRecordEntity = new StatisticRecord { Id = 0, QrId = 1, Count = 10, StreetcodeCoordinateId = 101 };
        var mappedStatisticRecordEntityAfterSave = new StatisticRecord { Id = 1, QrId = 1, Count = 10, StreetcodeCoordinateId = 101 };

        var expectedDto = new StatisticRecordDTO
        {
            Id = 1,
            QrId = 1,
            Count = 10,
            Address = "127.0.0.1",
            StreetcodeId = 1,
            StreetcodeCoordinateId = 101
        };

        _mockMapper.Setup(m => m.Map<StatisticRecord>(createDto))
            .Returns(statisticRecordEntity);

        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.CreateAsync(statisticRecordEntity))
            .ReturnsAsync(statisticRecordEntity)
            .Callback<StatisticRecord>(entity => entity.Id = mappedStatisticRecordEntityAfterSave.Id);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockMapper.Setup(m => m.Map<StatisticRecordDTO>(It.Is<StatisticRecord>(sr => sr.Id == mappedStatisticRecordEntityAfterSave.Id)))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);

        _mockRepositoryWrapper.Verify(r => r.StatisticRecordRepository.CreateAsync(statisticRecordEntity), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockLogger.Verify(l => l.LogInformation("CreateStatisticRecordCommand: Statistic record created successfully."), Times.Once);
    }

    [Fact]
    public async Task Handle_NullCreateDTO_ShouldReturnFail()
    {
        // Arrange
        var command = new CreateStatisticRecordCommand(null);
        var expectedErrorMessage = "Statistic record data is required.";

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);

        _mockLogger.Verify(l => l.LogWarning("CreateStatisticRecordCommand: StatisticRecordCreateDTO is null."), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.StatisticRecordRepository.CreateAsync(It.IsAny<StatisticRecord>()), Times.Never);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncFails_ShouldStillReturnSuccess_DueToCurrentHandlerLogic()
    {
        // Arrange
        var createDto = new StatisticRecordCreateDTO
        {
            QrId = 1,
            Count = 10,
            Address = "127.0.0.1",
            StreetcodeId = 1,
            StreetcodeCoordinateId = 101
        };
        var command = new CreateStatisticRecordCommand(createDto);

        var statisticRecordEntity = new StatisticRecord
        {
            QrId = createDto.QrId.Value,
            Count = createDto.Count,
            Address = createDto.Address,
            StreetcodeId = createDto.StreetcodeId,
            StreetcodeCoordinateId = createDto.StreetcodeCoordinateId
        };

        var expectedDtoAfterMapFromFailedSave = new StatisticRecordDTO
        {
            Id = 0,
            QrId = (int)createDto.QrId,
            Count = createDto.Count,
            Address = createDto.Address,
            StreetcodeId = createDto.StreetcodeId,
            StreetcodeCoordinateId = createDto.StreetcodeCoordinateId
        };

        _mockMapper.Setup(m => m.Map<StatisticRecord>(createDto))
            .Returns(statisticRecordEntity);

        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.CreateAsync(statisticRecordEntity))
            .ReturnsAsync(statisticRecordEntity);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        _mockMapper.Setup(m => m.Map<StatisticRecordDTO>(statisticRecordEntity))
            .Returns(expectedDtoAfterMapFromFailedSave);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert 
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDtoAfterMapFromFailedSave);
        _mockLogger.Verify(l => l.LogInformation("CreateStatisticRecordCommand: Statistic record created successfully."), Times.Once);
    }
}