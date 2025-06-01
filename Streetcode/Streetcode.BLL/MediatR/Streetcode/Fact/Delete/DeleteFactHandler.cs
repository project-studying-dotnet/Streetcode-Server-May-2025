using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Delete;

public class DeleteFactHandler : IRequestHandler<DeleteFactCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public DeleteFactHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, ICacheInvalidationService cacheInvalidationService)
    {
        _cacheInvalidationService = cacheInvalidationService;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteFactCommand request, CancellationToken cancellationToken)
    {
        int id = request.id;
        var fact = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(n => n.Id == id);

        if (fact == null)
        {
            string errorMsg = $"Couldn't find a fact with id: {request.id}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        if (fact.Image != null)
        {
            _repositoryWrapper.ImageRepository.Delete(fact.Image);
        }

        _repositoryWrapper.FactRepository.Delete(fact);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            _logger.LogInformation("DeleteFactCommand handled successfully");
            await _cacheInvalidationService.InvalidateAllCacheAsync(Constants.CacheSetKeys.Facts);
            return Result.Ok(Unit.Value);
        }
        else
        {
            string errorMsg = "Failed to delete a fact";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}