using AutoMapper;
using FluentAssertions;
using Moq;
using Repositories.Interfaces;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Media.Art.Create;

public class CreateArtHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IArtRepository> _mockArtRepository;
    private readonly Mock<IImageRepository> _mockImageRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly CreateArtHandler _handler;

    public CreateArtHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockArtRepository = new Mock<IArtRepository>();
        _mockImageRepository = new Mock<IImageRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockBlobService = new Mock<IBlobService>();
        _mockLoggerService = new Mock<ILoggerService>();
        _mockRepositoryWrapper.Setup(r => r.ArtRepository).Returns(_mockArtRepository.Object);
        _mockRepositoryWrapper.Setup(r => r.ImageRepository).Returns(_mockImageRepository.Object);
        _handler = new CreateArtHandler(
            _mockRepositoryWrapper.Object,
            _mockMapper.Object,
            _mockBlobService.Object,
            _mockLoggerService.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsOkResultWithArtDTO()
    {
        // Arrange
        var artCreateRequest = new ArtCreateRequestDTO
        {
            Title = "Test Art",
            Description = "Test Description",
            Image = new ImageFileBaseCreateDTO { BaseFormat = "validbase64", Extension = "png", Title = "ImageTitle", Alt = "ImageAlt" }
        };
        var command = new CreateArtCommand(artCreateRequest);
        var savedImageEntity = new ImageEntity { Id = 1, BlobName = "hashed.png", MimeType = "image/png" };
        var savedArtEntity = new ArtEntity { Id = 1, Title = "Test Art", ImageId = 1, Image = savedImageEntity };
        var artDto = new ArtDTO { Id = 1, Title = "Test Art", Image = new ImageDTO { Id = 1, BlobName = "hashed.png", Base64 = "base64image" } };

        _mockBlobService.Setup(s => s.SaveFileInStorageAsync(artCreateRequest.Image.BaseFormat, It.IsAny<string>(), artCreateRequest.Image.Extension))
            .ReturnsAsync("hashed");

        _mockRepositoryWrapper.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1); // Simulate successful save

        _mockImageRepository.Setup(r => r.CreateAsync(It.IsAny<ImageEntity>()))
            .Callback<ImageEntity>(img => img.Id = savedImageEntity.Id);

        _mockMapper.Setup(m => m.Map<ArtDTO>(It.IsAny<ArtEntity>()))
            .Returns(artDto);
        _mockBlobService.Setup(s => s.FindFileInStorageAsBase64Async(It.IsAny<string>()))
            .ReturnsAsync("base64image");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(artDto, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Handle_NullImageDataInRequest_ReturnsFailResult()
    {
        // Arrange
        var artCreateRequest = new ArtCreateRequestDTO { Title = "Test Art", Image = null };
        var command = new CreateArtCommand(artCreateRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Дані зображення є обов'язковими.");
    }

    [Fact]
    public async Task Handle_EmptyBaseFormatInImage_ReturnsFailResult()
    {
        // Arrange
        var artCreateRequest = new ArtCreateRequestDTO
        {
            Image = new ImageFileBaseCreateDTO { BaseFormat = "", Extension = "png" }
        };
        var command = new CreateArtCommand(artCreateRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Base64 рядок зображення є обов'язковим.");
    }

    [Fact]
    public async Task Handle_EmptyExtensionInImage_ReturnsFailResult()
    {
        // Arrange
        var artCreateRequest = new ArtCreateRequestDTO
        {
            Image = new ImageFileBaseCreateDTO { BaseFormat = "validbase64", Extension = "" }
        };
        var command = new CreateArtCommand(artCreateRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Розширення файлу зображення є обов'язковим.");
    }

    [Fact]
    public async Task Handle_BlobServiceSaveThrowsException_ThrowsInvalidOperationException()
    {
        // Arrange
        var artCreateRequest = new ArtCreateRequestDTO
        {
            Image = new ImageFileBaseCreateDTO { BaseFormat = "validbase64", Extension = "png" }
        };
        var command = new CreateArtCommand(artCreateRequest);
        var expectedExceptionMessage = "Blob service failed";

        _mockBlobService.Setup(s => s.SaveFileInStorageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(expectedExceptionMessage));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage(expectedExceptionMessage);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncForArtThrowsException_ThrowsException() // Renamed for clarity
    {
        // Arrange
        var artCreateRequest = new ArtCreateRequestDTO
        {
            Title = "Test Art",
            Image = new ImageFileBaseCreateDTO { BaseFormat = "validbase64", Extension = "png", Title = "ImageTitle" }
        };
        var command = new CreateArtCommand(artCreateRequest);
        var expectedExceptionMessage = "Simulated database error during art save";

        _mockBlobService.Setup(s => s.SaveFileInStorageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("hashedblobname");

        _mockImageRepository.Setup(r => r.CreateAsync(It.IsAny<ImageEntity>()))
            .Callback<ImageEntity>(img => img.Id = 1);

        _mockRepositoryWrapper.SetupSequence(r => r.SaveChangesAsync())
            .ReturnsAsync(1)
            .ThrowsAsync(new Exception(expectedExceptionMessage));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<Exception>())
            .WithMessage(expectedExceptionMessage);
    }
}
