using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create;

public class CreateFactHandler : IRequestHandler<CreateFactCommand, Result<FactDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public CreateFactHandler(
        IRepositoryWrapper repositoryWrapper, 
        IMapper mapper, 
        ILoggerService logger,
        ICacheInvalidationService cacheInvalidationService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<Result<FactDTO>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
    {
        var newFact = _mapper.Map<Entity>(request.NewFact);

        if (newFact is null)
        {
            const string errorMsg = "Invalid fact data provided. New Fact is null.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        if (newFact.StreetcodeId == 0)
        {
            const string errorMsg = "StreetcodeId is required.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        var duplicate = await _repositoryWrapper
            .FactRepository
            .GetFirstOrDefaultAsync(
                predicate: f =>
                    f.StreetcodeId == newFact.StreetcodeId &&
                    f.FactContent == newFact.FactContent);

        if (duplicate is not null)
        {
            const string errorMsg = "A fact with the same content already exists for this streetcode.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }

        newFact.ImageId = newFact.ImageId == 0 ? null : newFact.ImageId;
        newFact.Position = newFact.Position == 0 ? null : newFact.Position;

        var entity = await _repositoryWrapper.FactRepository.CreateAsync(newFact);
        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccessResult)
        {
            await _cacheInvalidationService.InvalidateAllCacheAsync(Constants.CacheSetKeys.Facts);
            return Result.Ok(_mapper.Map<FactDTO>(entity));
        }
        else
        {
            const string errorMsg = "Failed to save the fact.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }
    }
}