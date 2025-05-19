using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatRTests.Streetcode.RelatedTermTests.GetAllByTermId
{
    public class GetAllRelatedTermsByTermIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IRelatedTermRepository> _mockRelatedTermRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;

        public GetAllRelatedTermsByTermIdHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockRelatedTermRepository = new Mock<IRelatedTermRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();

            _mockRepositoryWrapper.Setup(x => x.RelatedTermRepository).Returns(_mockRelatedTermRepository.Object);
        }

        private async Task<Result<IEnumerable<RelatedTermDTO>>> ArrangeAndActAsync(int termId, IEnumerable<RelatedTerm> entityListToReturn, IEnumerable<RelatedTermDTO> dtoListToReturn)
        {
            var command = new GetAllRelatedTermsByTermIdQuery(termId);

            _mockRelatedTermRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()))
                .ReturnsAsync(entityListToReturn);

            _mockMapper.Setup(m => m.Map<IEnumerable<RelatedTermDTO>>(It.IsAny<IEnumerable<RelatedTerm>>()))
                .Returns(dtoListToReturn);

            var handler = new GetAllRelatedTermsByTermIdHandler(
                _mockMapper.Object,
                _mockRepositoryWrapper.Object,
                _mockLogger.Object);

            return await handler.Handle(command, CancellationToken.None);
        }

        // --- Successful Retrieval Tests ---

        [Fact]
        public async Task Handle_ExistingTerms_ShouldReturnSuccessResult()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            var dtoList = new List<RelatedTermDTO> { new RelatedTermDTO { Id = 1, TermId = termId, Word = "Word1" } };

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingTerms_ShouldReturnCorrectDtoValue()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            var dtoList = new List<RelatedTermDTO> { new RelatedTermDTO { Id = 1, TermId = termId, Word = "Word1" } };

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.Value.Should().BeEquivalentTo(dtoList);
        }

        [Fact]
        public async Task Handle_ExistingTerms_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            var dtoList = new List<RelatedTermDTO> { new RelatedTermDTO { Id = 1, TermId = termId, Word = "Word1" } };

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(
                    It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                    It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingTerms_ShouldCallMapperToMapEntityListToDtoList()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            var dtoList = new List<RelatedTermDTO> { new RelatedTermDTO { Id = 1, TermId = termId, Word = "Word1" } };

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockMapper.Verify(m => m.Map<IEnumerable<RelatedTermDTO>>(entityList), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingTerms_ShouldNotCallLoggerError()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            var dtoList = new List<RelatedTermDTO> { new RelatedTermDTO { Id = 1, TermId = termId, Word = "Word1" } };

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockLogger.Verify(logger => logger.LogError(It.IsAny<GetAllRelatedTermsByTermIdQuery>(), It.IsAny<string>()), Times.Never);
        }

        // --- Repository Returns Empty List Tests ---

        [Fact]
        public async Task Handle_EmptyRepositoryResult_ShouldReturnSuccessResult()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm>();
            var dtoList = new List<RelatedTermDTO>();

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyRepositoryResult_ShouldReturnEmptyDtoList()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm>();
            var dtoList = new List<RelatedTermDTO>();

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_EmptyRepositoryResult_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm>();
            var dtoList = new List<RelatedTermDTO>();

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(
                    It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                    It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyRepositoryResult_ShouldCallMapperToMapEmptyEntityListToEmptyDtoList()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm>();
            var dtoList = new List<RelatedTermDTO>();

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockMapper.Verify(m => m.Map<IEnumerable<RelatedTermDTO>>(entityList), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyRepositoryResult_ShouldNotCallLoggerError()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm>();
            var dtoList = new List<RelatedTermDTO>();

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockLogger.Verify(logger => logger.LogError(It.IsAny<GetAllRelatedTermsByTermIdQuery>(), It.IsAny<string>()), Times.Never);
        }


        // --- Repository Returns Null Tests ---

        [Fact]
        public async Task Handle_RepositoryReturnsNull_ShouldReturnFailureResult()
        {
            var termId = 1;
            IEnumerable<RelatedTerm> entityList = null;
            IEnumerable<RelatedTermDTO> dtoList = null;

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_RepositoryReturnsNull_ShouldContainCorrectErrorMessage()
        {
            var termId = 1;
            IEnumerable<RelatedTerm> entityList = null;
            IEnumerable<RelatedTermDTO> dtoList = null;
            var expectedErrorMessage = "Cannot get words by term id";

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        }

        [Fact]
        public async Task Handle_RepositoryReturnsNull_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            var termId = 1;
            IEnumerable<RelatedTerm> entityList = null;
            IEnumerable<RelatedTermDTO> dtoList = null;

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(
                    It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                    It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryReturnsNull_ShouldNotCallMapper()
        {
            var termId = 1;
            IEnumerable<RelatedTerm> entityList = null;
            IEnumerable<RelatedTermDTO> dtoList = null;

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockMapper.Verify(m => m.Map<IEnumerable<RelatedTermDTO>>(It.IsAny<IEnumerable<RelatedTerm>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RepositoryReturnsNull_ShouldCallLoggerError()
        {
            var termId = 1;
            IEnumerable<RelatedTerm> entityList = null;
            IEnumerable<RelatedTermDTO> dtoList = null;
            var expectedErrorMessage = "Cannot get words by term id";

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<GetAllRelatedTermsByTermIdQuery>(), expectedErrorMessage),
                Times.Once);
        }

        // --- Mapper Returns Null Tests ---

        [Fact]
        public async Task Handle_MapperReturnsNull_ShouldReturnFailureResult()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            IEnumerable<RelatedTermDTO> dtoList = null;

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_MapperReturnsNull_ShouldContainCorrectErrorMessage()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            IEnumerable<RelatedTermDTO> dtoList = null;
            var expectedErrorMessage = "Cannot create DTOs for related words!";

            var result = await ArrangeAndActAsync(termId, entityList, dtoList);

            result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);
        }

        [Fact]
        public async Task Handle_MapperReturnsNull_ShouldCallRelatedTermRepositoryGetAllAsync()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            IEnumerable<RelatedTermDTO> dtoList = null;

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockRelatedTermRepository.Verify(
                repo => repo.GetAllAsync(
                    It.IsAny<Expression<System.Func<RelatedTerm, bool>>>(),
                    It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MapperReturnsNull_ShouldCallMapperToMapEntityListToDtoList()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            IEnumerable<RelatedTermDTO> dtoList = null;

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockMapper.Verify(m => m.Map<IEnumerable<RelatedTermDTO>>(entityList), Times.Once);
        }

        [Fact]
        public async Task Handle_MapperReturnsNull_ShouldCallLoggerError()
        {
            var termId = 1;
            var entityList = new List<RelatedTerm> { new RelatedTerm { Id = 1, TermId = termId, Word = "Word1" } };
            IEnumerable<RelatedTermDTO> dtoList = null;
            var expectedErrorMessage = "Cannot create DTOs for related words!";

            await ArrangeAndActAsync(termId, entityList, dtoList);

            _mockLogger.Verify(
                logger => logger.LogError(It.IsAny<GetAllRelatedTermsByTermIdQuery>(), expectedErrorMessage),
                Times.Once);
        }
    }
}
