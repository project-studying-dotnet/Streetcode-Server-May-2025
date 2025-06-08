using Ardalis.Specification;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetById;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using TransactionLinkEntity = Streetcode.DAL.Entities.Transactions.TransactionLink;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Transactions.TransactionLink.GetById;

public class GetTransactLinkByIdHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly GetTransactLinkByIdHandler _handler;

    public GetTransactLinkByIdHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new GetTransactLinkByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TransactLinkExists_ShouldReturnTransactLink()
    {
        // Arrange
        var transactLink = new TransactionLinkEntity { Id = 1 };
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

        var query = new GetTransactLinkByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(transactLinkDto);
        _mapperMock.Verify(m => m.Map<TransactLinkDTO>(It.IsAny<TransactionLinkEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TransactLinkIsNull_ShouldReturnFail()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(r => r.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TransactionLinkEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TransactionLinkEntity>, IIncludableQueryable<TransactionLinkEntity, object>>>()))
            .ReturnsAsync((TransactionLinkEntity)null);

        var query = new GetTransactLinkByIdQuery(1);
        var expectedMessage = $"Cannot find any transaction link with corresponding id: {query.Id}";

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(expectedMessage);
        _loggerMock.Verify(l => l.LogError(query, expectedMessage), Times.Once);
        _mapperMock.Verify(m => m.Map<TransactLinkDTO>(It.IsAny<TransactionLinkEntity>()), Times.Never);
    }
}
