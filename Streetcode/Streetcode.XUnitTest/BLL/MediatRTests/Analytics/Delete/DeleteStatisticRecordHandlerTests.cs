using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Streetcode.BLL.MediatR.Analytics.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Analytics.Create;
using AutoMapper;
using MediatR;

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
        _mockMediator = new Mock<IMediator>();

        _handler = new CreateStatisticRecordHandler(
            _mockMediator.Object,
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
        Assert.True(result.IsSuccess);
        _mockRepositoryWrapper.Verify(r => r.StatisticRecordRepository.Delete(record), Times.Once);
        _mockRepositoryWrapper.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RecordNotFound_ShouldReturnFailure()
    {
        // Arrange
        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StatisticRecord, bool>>>(),
            null))
            .ReturnsAsync((StatisticRecord?)null);

        var handler = CreateHandler();
        var command = new DeleteStatisticRecordCommand(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Errors[0].Message);
        _mockRepositoryWrapper.Verify(r => r.StatisticRecordRepository.Delete(It.IsAny<StatisticRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var record = new StatisticRecord { Id = 1 };
        _mockRepositoryWrapper.Setup(r => r.StatisticRecordRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StatisticRecord, bool>>>(),
            null))
            .ReturnsAsync(record);

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new Exception("DB error"));

        var handler = CreateHandler();
        var command = new DeleteStatisticRecordCommand(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Error deleting", result.Errors[0].Message);
    }
}
