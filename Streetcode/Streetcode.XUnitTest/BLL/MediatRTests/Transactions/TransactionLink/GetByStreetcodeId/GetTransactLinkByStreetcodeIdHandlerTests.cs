using Ardalis.Specification;
using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;

using TransactionLinkEntity = Streetcode.DAL.Entities.Transactions.TransactionLink;
using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Transactions.TransactionLink.GetByStreetcodeId;

public class GetTransactLinkByStreetcodeIdHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly GetTransactLinkByStreetcodeIdHandler _handler;

    public GetTransactLinkByStreetcodeIdHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new GetTransactLinkByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TransactLinkExists_ShouldReturnTransactLinkDto()
    {
        // Arrange
        int streetcodeId = 5;
        var transactLink = new TransactionLinkEntity { Id = 1, StreetcodeId = streetcodeId };
        var transactLinkDto = new TransactLinkDTO { Id = 1 };

        _repositoryWrapperMock
            .Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
            .ReturnsAsync(transactLink);

        _mapperMock
            .Setup(m => m.Map<TransactLinkDTO>(
                It.IsAny<TransactionLinkEntity>()))
            .Returns(transactLinkDto);

        var query = new GetTransactLinkByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(transactLinkDto);
        _mapperMock.Verify(m => m.Map<TransactLinkDTO?>(transactLink), Times.Once);
        _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TransactLinkNull_StreetcodeExists_ShouldReturnNullResult()
    {
        // Arrange
        int streetcodeId = 5;

        _repositoryWrapperMock
           .Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
               It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
           .ReturnsAsync((TransactionLinkEntity)null);

        _repositoryWrapperMock
            .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeEntity>, IIncludableQueryable<StreetcodeEntity, object>>>()))
            .ReturnsAsync(new StreetcodeEntity { Id = streetcodeId });

        _mapperMock
            .Setup(m => m.Map<TransactLinkDTO?>(null))
            .Returns((TransactLinkDTO?)null);

        var query = new GetTransactLinkByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        _mapperMock.Verify(m => m.Map<TransactLinkDTO?>(null), Times.Once);
    }

    [Fact]
    public async Task Handle_TransactLinkNull_StreetcodeNull_ShouldReturnFail()
    {
        // Arrange
        int streetcodeId = 5;
        var expectedMessage = $"Cannot find a transaction link by a streetcode id: {streetcodeId}, because such streetcode doesn`t exist";

        _repositoryWrapperMock
           .Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
               It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
           .ReturnsAsync((TransactionLinkEntity)null);

        _repositoryWrapperMock
            .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeEntity>, IIncludableQueryable<StreetcodeEntity, object>>>()))
            .ReturnsAsync((StreetcodeEntity)null);

        var query = new GetTransactLinkByStreetcodeIdQuery(streetcodeId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedMessage);
        _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        _mapperMock.Verify(m => m.Map<TransactLinkDTO?>(It.IsAny<TransactionLinkEntity>()), Times.Never);
    }



























    //[Fact]
    //public async Task Handle_ShouldReturnTransactLinks_WhenTransactLinksExist()
    //{
    //    // Arrange
    //    var streetcodeId = 1;

    //    var partner = new TransactionLinkEntity { Id = 1, Streetcodes = new List<StreetcodeContent> { new StreetcodeContent { Id = streetcodeId } } };
    //    var partnerDto = new TransactLinkDTO { Id = 1 };

    //    _repositoryWrapperMock
    //        .Setup(r => r.TransactLinksRepository.ListAsync(
    //            It.IsAny<ISpecification<TransactionLinkEntity>>(),
    //            It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new List<TransactionLinkEntity> { partner });

    //    _repositoryWrapperMock
    //        .Setup(r => r.StreetcodeRepository.GetSingleOrDefaultAsync(
    //            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
    //            null))
    //        .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

    //    _repositoryWrapperMock
    //        .Setup(r => r.StreetcodeRepository.GetBySpecAsync(
    //            It.IsAny<ISpecification<StreetcodeContent>>(),
    //            It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

    //    _mapperMock
    //       .Setup(m => m.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<TransactionLinkEntity>>()))
    //       .Returns(new List<TransactLinkDTO> { partnerDto });

    //    var query = new GetTransactLinksByStreetcodeIdQuery(streetcodeId);

    //    // Act
    //    var result = await _handler.Handle(query, CancellationToken.None);

    //    // Assert
    //    Assert.True(result.IsSuccess);
    //    var returnedTransactLink = Assert.Single(result.Value);
    //    Assert.Equal(partnerDto.Id, returnedTransactLink.Id);
    //}

    //[Fact]
    //public async Task Handle_ShouldReturnSuccess_WhenTransactLinksListIsEmpty()
    //{
    //    // Arrange
    //    var streetcodeId = 1;

    //    _repositoryWrapperMock
    //     .Setup(r => r.StreetcodeRepository.GetBySpecAsync(
    //         It.IsAny<ISpecification<StreetcodeContent>>(),
    //         It.IsAny<CancellationToken>()))
    //     .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

    //    _repositoryWrapperMock
    //        .Setup(r => r.TransactLinksRepository.ListAsync(
    //            It.IsAny<ISpecification<TransactionLinkEntity>>(),
    //            It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(new List<TransactionLinkEntity>());

    //    _mapperMock
    //        .Setup(m => m.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<TransactionLinkEntity>>()))
    //        .Returns(new List<TransactLinkDTO>());

    //    var query = new GetTransactLinksByStreetcodeIdQuery(1);

    //    // Act
    //    var result = await _handler.Handle(query, CancellationToken.None);

    //    // Assert
    //    Assert.True(result.IsSuccess);
    //    Assert.Empty(result.Value);
    //}

    //[Fact]
    //public async Task Handle_ShouldReturnFail_WhenStreetcodeIsNotExist()
    //{
    //    // Arrange
    //    var streetcodeId = 1;

    //    _repositoryWrapperMock
    // .Setup(r => r.StreetcodeRepository.GetBySpecAsync(
    //     It.IsAny<ISpecification<StreetcodeContent>>(),
    //     It.IsAny<CancellationToken>()))
    //        .ReturnsAsync((StreetcodeContent)null);

    //    var query = new GetTransactLinksByStreetcodeIdQuery(streetcodeId);
    //    var expectedMessage = $"Cannot find any partners with corresponding streetcode id: {query.StreetcodeId}";

    //    // Act
    //    var result = await _handler.Handle(query, CancellationToken.None);

    //    // Assert
    //    Assert.False(result.IsSuccess);
    //    Assert.Equal(expectedMessage, result.Errors.First().Message);
    //    _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
    //}

    //[Fact]
    //public async Task Handle_ShouldReturnFail_WhenTransactLinksListIsNull()
    //{
    //    // Arrange
    //    var streetcodeId = 1;

    //    _repositoryWrapperMock
    //            .Setup(r => r.StreetcodeRepository.GetBySpecAsync(
    //                It.IsAny<ISpecification<StreetcodeContent>>(),
    //                It.IsAny<CancellationToken>()))
    //            .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

    //    _repositoryWrapperMock
    //        .Setup(r => r.TransactLinksRepository.ListAsync(
    //            It.IsAny<ISpecification<TransactionLinkEntity>>(),
    //            It.IsAny<CancellationToken>()))
    //        .ReturnsAsync((List<TransactionLinkEntity>)null);

    //    var query = new GetTransactLinksByStreetcodeIdQuery(streetcodeId);
    //    var expectedMessage = $"Cannot find partners by a streetcode id: {query.StreetcodeId}";

    //    // Act
    //    var result = await _handler.Handle(query, CancellationToken.None);

    //    // Assert
    //    Assert.False(result.IsSuccess);
    //    Assert.Equal(expectedMessage, result.Errors.First().Message);
    //    _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
    //}
}
