using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.MainPage.Create;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.MainPage.Create;

public class CreateMainStreetcodeHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepo = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILoggerService> _mockLogger = new();

    private readonly CreateMainStreetcodeHandler _handler;

    public CreateMainStreetcodeHandlerTests()
    {
        _handler = new CreateMainStreetcodeHandler(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_StreetcodeExists_UpdatesBriefDescription()
    {
        var dto = new StreetcodeMainPageCreateDTO { StreetcodeId = 1, BriefDescription = "Short" };
        var streetcode = new StreetcodeContent { Id = 1 };

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                 .ReturnsAsync(streetcode);

        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _handler.Handle(new CreateMainStreetcodeCommand(dto), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_StreetcodeNotFound_ReturnsFailure()
    {
        var dto = new StreetcodeMainPageCreateDTO { StreetcodeId = 1, BriefDescription = "Short" };

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                 .ReturnsAsync((StreetcodeContent?)null);

        var result = await _handler.Handle(new CreateMainStreetcodeCommand(dto), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Streetcode not found.");
        _mockLogger.Verify(l => l.LogError(dto, "Streetcode not found."), Times.Once);
    }

    [Fact]
    public async Task Handle_SaveChangesFailed_ReturnsFailure()
    {
        var dto = new StreetcodeMainPageCreateDTO { StreetcodeId = 1, BriefDescription = "Short" };
        var streetcode = new StreetcodeContent { Id = 1 };

        _mockRepo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                 .ReturnsAsync(streetcode);

        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var result = await _handler.Handle(new CreateMainStreetcodeCommand(dto), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Failed to save brief description.");
        _mockLogger.Verify(l => l.LogError(dto, "Failed to save brief description."), Times.Once);
    }
}