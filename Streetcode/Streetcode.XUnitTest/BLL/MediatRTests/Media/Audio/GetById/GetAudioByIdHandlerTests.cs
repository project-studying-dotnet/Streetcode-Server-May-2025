using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Audio.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using EntAudio = Streetcode.DAL.Entities.Media.Audio;
using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Audio.GetById;

public class GetAudioByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repo = new();
    private readonly Mock<IMapper> _map = new();
    private readonly Mock<IBlobService> _blob = new();
    private readonly Mock<ILoggerService> _log = new();
    private readonly GetAudioByIdHandler _handler;
    private const string Base64Content = "b64";

    public GetAudioByIdHandlerTests() =>
        _handler = new GetAudioByIdHandler(_repo.Object, _map.Object, _blob.Object, _log.Object);

    [Fact]
    public async Task Success_ReturnsDto()
    {
        // Arrange
        var audio = CreateAudio(5, "z.mp3");
        var dto = CreateDto(5, "z.mp3");
        SetupDependencies(audio, dto);

        // Act
        var result = await _handler.Handle(new GetAudioByIdQuery(5), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Base64.Should().Be(Base64Content);
    }

    [Fact]
    public async Task NotFound_Fails()
    {
        // Arrange
        SetupRepositoryToReturnNull();

        // Act
        var result = await _handler.Handle(new GetAudioByIdQuery(404), default);

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task Mapper_CalledOnce()
    {
        // Arrange
        var audio = CreateAudio(2, "b.mp3");
        var dto = CreateDto(2, "b.mp3");
        SetupDependencies(audio, dto);

        // Act
        await _handler.Handle(new GetAudioByIdQuery(2), default);

        // Assert
        _map.Verify(m => m.Map<AudioDTO>(audio), Times.Once);
    }

    [Fact]
    public async Task Blob_CalledOnce()
    {
        // Arrange
        var audio = CreateAudio(6, "c.mp3");
        var dto = CreateDto(6, "c.mp3");
        SetupDependencies(audio, dto);

        // Act
        await _handler.Handle(new GetAudioByIdQuery(6), default);

        // Assert
        _blob.Verify(b => b.FindFileInStorageAsBase64Async("c.mp3"), Times.Once);
    }

    private EntAudio CreateAudio(int id, string blobName) => 
        new() { Id = id, BlobName = blobName };

    private AudioDTO CreateDto(int id, string blobName) => 
        new() { Id = id, BlobName = blobName };

    private void SetupDependencies(EntAudio audio, AudioDTO dto)
    {
        _repo.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<EntAudio, bool>>>(),
                It.IsAny<Func<IQueryable<EntAudio>, IIncludableQueryable<EntAudio, object>>>()))
            .ReturnsAsync(audio);
        _map.Setup(m => m.Map<AudioDTO>(audio)).Returns(dto);
        _blob.Setup(b => b.FindFileInStorageAsBase64Async(audio.BlobName)).ReturnsAsync(Base64Content);
    }

    private void SetupRepositoryToReturnNull()
    {
        _repo.Setup(r => r.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<EntAudio, bool>>>(),
                It.IsAny<Func<IQueryable<EntAudio>, IIncludableQueryable<EntAudio, object>>>()))
            .ReturnsAsync((EntAudio?)null);
    }
}