using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Analytics.Create;
using Streetcode.BLL.MediatR.Analytics.Delete;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Analytics.Delete;

public class DeleteStatisticRecordHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CreateStatisticRecordHandler _handler;

    public DeleteStatisticRecordHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();

        _handler = new CreateStatisticRecordHandler(
            _mockRepositoryWrapper.Object,
            _mockLogger.Object,
            _mockMapper.Object);
    }

    private DeleteStatisticRecordHandler CreateHandler()
    {
        return new DeleteStatisticRecordHandler(
            _mockRepositoryWrapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_RecordExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var record = new StatisticRecord { Id = 1 };
        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StatisticRecord, bool>>>(),
            null))
            .ReturnsAsync(record);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var handler = CreateHandler();
        var command = new DeleteStatisticRecordCommand(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _mockRepositoryWrapper.Verify(r => r.StatisticRecordRepository.Delete(record), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailResult_WhenRecordDoesNotExist()
    {
        // Arrange
        var command = new DeleteStatisticRecordCommand(42);

        _mockRepositoryWrapper.Setup(repo =>
            repo.StatisticRecordRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StatisticRecord, bool>>>(),
                null))
            .ReturnsAsync((StatisticRecord?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("No statistic record found", result.Errors[0].Message);

        _mockRepositoryWrapper.Verify(repo =>
            repo.StatisticRecordRepository.Delete(It.IsAny<StatisticRecord>()), Times.Never);

        _mockLogger.Verify(logger =>
            logger.LogError(command, It.Is<string>(msg => msg.Contains("No statistic record"))), Times.Once);
    }
}