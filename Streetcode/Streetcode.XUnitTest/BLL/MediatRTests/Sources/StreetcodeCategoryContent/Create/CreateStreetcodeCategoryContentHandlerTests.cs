using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Sources.StreetcodeCategoryContent.Create;

public class CreateStreetcodeCategoryContentHandlerTests
{
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILoggerService> _mockLogger = new();
    private readonly Mock<IRepositoryWrapper> _mockRepository = new();
    private readonly CreateStreetcodeCategoryContentHandler _handler;
    
    public CreateStreetcodeCategoryContentHandlerTests()
    {
        _handler = new CreateStreetcodeCategoryContentHandler(
            _mockLogger.Object, 
            _mockMapper.Object, 
            _mockRepository.Object);
    }

    [Fact]
    public async Task Handle_GivenValidCreateCategoryContentDto_ReturnsResultSuccessfully()
    {
        // Arrange
        var inputDto = new CategoryContentCreateDTO
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        var returnsDto = new StreetcodeCategoryContentDTO
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        
        _mockMapper.Setup(m =>
            m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(inputDto))
            .Returns(entity);
        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);
        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity))
            .ReturnsAsync(entity);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockMapper.Setup(m => m.Map<StreetcodeCategoryContentDTO>(entity))
            .Returns(returnsDto);
        
        // Act
        var result = await _handler.Handle(new CreateStreetcodeCategoryContentCommand(inputDto), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(returnsDto);
        _mockMapper.Verify(m => m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(inputDto), Times.Once);
        _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Once);
        _mockRepository.Verify(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_SameCategoryContentExist_ReturnsErrorMessage()
    {
        // Arrange
        var inputDto = new CategoryContentCreateDTO
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        _mockMapper.Setup(m =>
                m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(inputDto))
            .Returns(entity);
        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
            .ReturnsAsync(entity);
        
        // Act
        var result = await _handler.Handle(new CreateStreetcodeCategoryContentCommand(inputDto), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Message == "A Category with the same content already exists for this streetcode.");
        _mockLogger.Verify(x => x.LogError(inputDto, It.IsAny<string>()), Times.Once);
        _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Never);
        _mockRepository.Verify(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity), Times.Never);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_CreateFailed_ReturnsErrorMessage()
    {
        // Arrange
        var inputDto = new CategoryContentCreateDTO
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        var entity = new DAL.Entities.Sources.StreetcodeCategoryContent
        {
            StreetcodeId = 1,
            SourceLinkCategoryId = 1
        };
        _mockMapper.Setup(m =>
                m.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(inputDto))
            .Returns(entity);
        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Sources.StreetcodeCategoryContent, bool>>>(), null))
            .ReturnsAsync((DAL.Entities.Sources.StreetcodeCategoryContent)null);
        _mockRepository.Setup(r => r.StreetcodeCategoryContentRepository.CreateAsync(entity))
            .ReturnsAsync(entity);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);
        
        // Act
        var result = await _handler.Handle(new CreateStreetcodeCategoryContentCommand(inputDto), CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.Message == "Failed to save the category content.");
        _mockLogger.Verify(x => x.LogError(inputDto, It.IsAny<string>()), Times.Once);
        _mockMapper.Verify(m => m.Map<StreetcodeCategoryContentDTO>(entity), Times.Never);
    }
}