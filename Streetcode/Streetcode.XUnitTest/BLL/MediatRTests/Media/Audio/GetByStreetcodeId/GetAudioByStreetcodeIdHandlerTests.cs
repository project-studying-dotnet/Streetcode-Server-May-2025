using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using EntAudio = Streetcode.DAL.Entities.Media.Audio;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.GetByStreetcodeId;

public class GetAudioByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repo = new();
    private readonly Mock<IMapper> _map = new();
    private readonly Mock<IBlobService> _blob = new();
    private readonly Mock<ILoggerService> _log = new();
    private readonly GetAudioByStreetcodeIdQueryHandler _handler;

    public GetAudioByStreetcodeIdHandlerTests() =>
        _handler = new GetAudioByStreetcodeIdQueryHandler(_repo.Object, _map.Object, _blob.Object, _log.Object);

    [Fact]
    public async Task Handle_WithAudio_ReturnsSuccessResult()
    {
        // Arrange
        var audio = new EntAudio { Id = 1, BlobName = "c.mp3" };
        var street = new StreetcodeContent { Id = 10, Audio = audio };
        var dto = new AudioDTO { Id = 1, BlobName = "c.mp3" };

        SetupRepositoryMock(street);
        SetupMapperMock(audio, dto);
        SetupBlobServiceMock("c.mp3", "b64");

        // Act
        var result = await _handler.Handle(new GetAudioByStreetcodeIdQuery(10), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Base64.Should().Be("b64");
    }

    [Fact]
    public async Task Handle_WithNoAudio_ReturnsSuccessWithNull()
    {
        // Arrange
        var street = new StreetcodeContent { Id = 11, Audio = null };
        SetupRepositoryMock(street);

        // Act
        var result = await _handler.Handle(new GetAudioByStreetcodeIdQuery(11), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_StreetcodeNotFound_ReturnsFailure() 
    {
        // Arrange
        SetupRepositoryMock(null);

        // Act
        var result = await _handler.Handle(new GetAudioByStreetcodeIdQuery(404), default);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AudioExists_MapperCalledAtLeastOnce()
    {
        // Arrange
        var audio = new EntAudio { Id = 2, BlobName = "d.mp3" };
        var street = new StreetcodeContent { Id = 12, Audio = audio };
        var dto = new AudioDTO { Id = 2, BlobName = "d.mp3" };

        SetupRepositoryMock(street);
        SetupMapperMock(audio, dto);
        SetupBlobServiceMock("d.mp3", "b64");

        // Act
        await _handler.Handle(new GetAudioByStreetcodeIdQuery(12), default);

        // Assert
        _map.Verify(m => m.Map<AudioDTO>(audio), Times.AtLeastOnce);
    }

    private void SetupRepositoryMock(StreetcodeContent? street)
    {
        _repo.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent,bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeContent>,IIncludableQueryable<StreetcodeContent,object>>>()))
        .ReturnsAsync(street);
    }

    private void SetupMapperMock(EntAudio audio, AudioDTO dto)
    {
        _map.Setup(m => m.Map<AudioDTO>(audio)).Returns(dto);
    }

    private void SetupBlobServiceMock(string blobName, string base64)
    {
        _blob.Setup(b => b.FindFileInStorageAsBase64Async(blobName)).ReturnsAsync(base64);
    }
}