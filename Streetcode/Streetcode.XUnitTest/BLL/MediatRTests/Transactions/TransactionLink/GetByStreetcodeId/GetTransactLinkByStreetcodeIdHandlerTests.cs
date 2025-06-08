using AutoMapper;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
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
}
