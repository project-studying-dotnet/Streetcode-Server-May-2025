using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.News.SortedByDateTime;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.NewsTests;

public class SortedByDateTimeHandlerTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLoggerService;
    private readonly Mock<IRepositoryWrapper> _mockRepository;
    private readonly Mock<IBlobService> _mockBlobService;
    private readonly SortedByDateTimeHandler _handler;

    public SortedByDateTimeHandlerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockLoggerService = new Mock<ILoggerService>();
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockBlobService = new Mock<IBlobService>();
        _handler = new SortedByDateTimeHandler(_mockRepository.Object, _mockMapper.Object, _mockBlobService.Object, _mockLoggerService.Object);
    }

    [Fact]
    public async Task Handle_NewsWIthImage_ShouldReturnAllSortedNewsSuccess()
    {
        // Arrange
        SetUpMockRepository(GetNewsCollection());
        _mockMapper.Setup(x => x.Map<IEnumerable<NewsDTO>>(It.IsAny<IEnumerable<News>>()))
            .Returns(GetNewsDtoCollection());
        var base64Image = "base64Image";
        SetUpMockBlobService(base64Image);

        // Act
        var result = await _handler.Handle(new SortedByDateTimeQuery(), CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.NewsRepository.GetAllAsync(
            null,
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>?>()));
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64Async(It.IsAny<string?>()), Times.Exactly(2));
        result.Value.Where(x => x.Image is not null)
            .All(x => x.Image.Base64 == base64Image).Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NewsWithoutImage_ShouldReturnAllSortedNewsSuccessfully()
    {
        // Arrange
        SetUpMockRepository(GetNewsCollection());
        _mockMapper.Setup(x => x.Map<IEnumerable<NewsDTO>>(It.IsAny<IEnumerable<News>>()))
            .Returns(GetNewsDtoCollectionWithoutImages());
        SetUpMockBlobService(null);

        // Act
        var result = await _handler.Handle(new SortedByDateTimeQuery(), CancellationToken.None);

        // Assert
        _mockRepository.Verify(r => r.NewsRepository.GetAllAsync(
            null,
            It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>?>()));
        _mockBlobService.Verify(x => x.FindFileInStorageAsBase64Async(It.IsAny<string?>()), Times.Never);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NullNewsCollection_ShouldReturnError()
    {
        // Arrange
        var errorMessage = "There are no news in the database";
        SetUpMockRepository(null);

        // Act
        var result = await _handler.Handle(new SortedByDateTimeQuery(), CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Message.Should().Be(errorMessage);
    }

    private IEnumerable<News> GetNewsCollection()
    {
        return new List<News>
        {
            new ()
            {
                Id = 1,
                Title = "Title",
                Text = "Text",
                ImageId = 1,
                URL = "/test",
                CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new ()
            {
                Id = 2,
                Title = "Title2",
                Text = "Text2",
                ImageId = 2,
                URL = "/test2",
                CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
        };
    }

    private IEnumerable<NewsDTO> GetNewsDtoCollection()
    {
        return new List<NewsDTO>
        {
            new ()
            {
                Id = 1,
                Title = "Title",
                Text = "Text",
                ImageId = 1,
                URL = "/test",
                Image = new ImageDTO
                {
                    Id = 1,
                    BlobName = "testblob",
                    MimeType = "image/png",
                },
                CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new ()
            {
                Id = 2,
                Title = "Title2",
                Text = "Text2",
                ImageId = 2,
                URL = "/test2",
                Image = new ImageDTO
                {
                    Id = 2,
                    BlobName = "testblob2",
                    MimeType = "image/png2",
                },
                CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
        };
    }

    private IEnumerable<NewsDTO> GetNewsDtoCollectionWithoutImages()
    {
        return new List<NewsDTO>
        {
            new ()
            {
                Id = 1,
                Title = "Title",
                Text = "Text",
                ImageId = 1,
                URL = "/test",
                CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new ()
            {
                Id = 2,
                Title = "Title2",
                Text = "Text2",
                ImageId = 2,
                URL = "/test2",
                CreationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
        };
    }

    private void SetUpMockRepository(IEnumerable<News> testNews)
    {
        _mockRepository.Setup(x => x.NewsRepository.GetAllAsync(
                null,
                It.IsAny<Func<IQueryable<News>, IIncludableQueryable<News, object>>?>()))
            .ReturnsAsync(testNews);
    }

    private void SetUpMockBlobService(string base64String)
    {
        _mockBlobService.Setup(x => x.FindFileInStorageAsBase64Async(It.IsAny<string>()))
            .ReturnsAsync(base64String);
    }
}