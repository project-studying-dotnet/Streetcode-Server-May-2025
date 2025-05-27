using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Reorder;

public class ReorderFactsHandler : IRequestHandler<ReorderFactsCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public ReorderFactsHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(ReorderFactsCommand request, CancellationToken cancellationToken)
    {
        if (!request.FactReorderDtos.Any())
        {
            string errorMsg = $"The list of new fact items cannot be empty.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        var duplicateIds = request.FactReorderDtos
            .GroupBy(d => d.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateIds.Count != 0)
        {
            string errorMsg = $"Duplicate FactId found: {string.Join(", ", duplicateIds)}";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        var positions = request
            .FactReorderDtos
            .Select(d => d.NewPosition)
            .OrderBy(x => x)
            .ToArray();
        if (!positions.SequenceEqual(Enumerable.Range(1, positions.Length)))
        {
            string errorMsg = $"Positions must be unique and go from 1 to the number of facts in a row.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        var facts = _repositoryWrapper
            .FactRepository
            .FindAll(f => f.StreetcodeId == request.StreetcodeId)
            .ToList();
        if (facts.Count == 0)
        {
            const string errorMsg = "No facts found for the specified streetcode ID.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        foreach (var dto in request.FactReorderDtos)
        {
            var fact = facts.FirstOrDefault(f => f.Id == dto.Id);
            if (fact is null)
            {
                string errorMsg = $"Fact with ID {dto.Id} not found for the specified streetcode ID.";
                _logger.LogError(request, errorMsg);

                return Result.Fail(new Error(errorMsg));
            }

            fact.Position = dto.NewPosition;
        }

        _repositoryWrapper.FactRepository.UpdateRange(facts);

        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccessResult)
        {
            return Result.Ok(Unit.Value);
        }
        else
        {
            const string errorMsg = "Cannot save changes in the database after facts reordering!";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }
    }
}