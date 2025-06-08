using Ardalis.Specification;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TransactionLinkEntity = Streetcode.DAL.Entities.Transactions.TransactionLink;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Transactions.TransactionLink.GetAll;

public class GetAllTransactLinksHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly GetAllTransactLinksHandler _handler;

    public GetAllTransactLinksHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new GetAllTransactLinksHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTransactLinksExist_ShouldReturnTransactLinks()
    {
        // Arrange
        var transactionLinks = new List<TransactionLinkEntity> { new TransactionLinkEntity { Id = 1 }, new TransactionLinkEntity { Id = 2 } };
        var transactionLinksDto = new List<TransactLinkDTO> { new TransactLinkDTO { Id = 1 }, new TransactLinkDTO { Id = 2 } };

        _repositoryWrapperMock
            .Setup(r => r.TransactLinksRepository.GetAllAsync(
                It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
            .ReturnsAsync(transactionLinks);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<TransactLinkDTO>>(
                It.IsAny<IEnumerable<TransactionLinkEntity>>()))
            .Returns(transactionLinksDto);

        var query = new GetAllTransactLinksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(transactionLinksDto);
        _mapperMock.Verify(m => m.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<TransactionLinkEntity>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyTransactLinksList_ShouldReturnSuccess()
    {
        // Arrange
        var emptyList = new List<TransactionLinkEntity>();
        var emptyDtos = new List<TransactLinkDTO>();

        _repositoryWrapperMock
            .Setup(r => r.TransactLinksRepository.GetAllAsync(
                It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
            .ReturnsAsync(emptyList);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<TransactLinkDTO>>(emptyList))
            .Returns(emptyDtos);

        var query = new GetAllTransactLinksQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NullTransactLinksList_ShouldReturnFail()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(r => r.TransactLinksRepository.GetAllAsync(
                It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
            .ReturnsAsync((List<TransactionLinkEntity>)null);

        var query = new GetAllTransactLinksQuery();
        var expectedMessage = "Cannot find any transaction link";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedMessage);
        _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<TransactionLinkEntity>>()), Times.Never);
    }
}
