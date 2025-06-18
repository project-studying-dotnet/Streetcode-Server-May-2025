using Moq;
using Xunit;
using FluentAssertions;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.MediatR.Streetcode.MainPage.Delete;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using System.Linq.Expressions;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.MainPage.Delete;
public class DeleteMainStreetcodeHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo = new();
    private readonly Mock<ILoggerService> _mockLogger = new();
    private readonly DeleteMainStreetcodeHandler _handler;

    public DeleteMainStreetcodeHandlerTests()
    {
        _handler = new DeleteMainStreetcodeHandler(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_StreetcodeExists_RemovesBriefDescription()
    {
        var dto = new StreetcodeMainPageDeleteDTO { StreetcodeId = 1 };
        var streetcode = new StreetcodeContent { Id = 1, BriefDescription = "Some description" };

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(streetcode);

        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteMainStreetcodeCommand(dto), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        streetcode.BriefDescription.Should().BeNull();
    }

    [Fact]
    public async Task Handle_StreetcodeNotFound_ReturnsFailure()
    {
        var dto = new StreetcodeMainPageDeleteDTO { StreetcodeId = 1 };

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent?)null);

        var result = await _handler.Handle(new DeleteMainStreetcodeCommand(dto), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        _mockLogger.Verify(l => l.LogError(dto, "Streetcode not found."), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFailed_ReturnsFailure()
    {
        var dto = new StreetcodeMainPageDeleteDTO { StreetcodeId = 1 };
        var streetcode = new StreetcodeContent { Id = 1, BriefDescription = "Text" };

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync(streetcode);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var result = await _handler.Handle(new DeleteMainStreetcodeCommand(dto), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        _mockLogger.Verify(l => l.LogError(dto, "Failed to remove brief description."), Times.Once);
    }
}