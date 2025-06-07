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
    private readonly FactValidator _validator;

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
        _validator = new FactValidator(_logger);
    }

    public async Task<Result<FactDTO>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
    {
        var newFact = _mapper.Map<Entity>(request.NewFact);

        var validation = _validator.Validation(request, newFact);

        if (validation != null)
        {
            return validation;
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
            return _validator.LogAndFail(request, errorMsg);
        }

        newFact.ImageId = newFact.ImageId == 0 ? null : newFact.ImageId;
        newFact.Position = newFact.Position == 0 ? null : newFact.Position;

        var entity = await _repositoryWrapper.FactRepository.CreateAsync(newFact);
        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccessResult)
        {
            await _cacheInvalidationService.InvalidateCacheAsync(Constants.CacheSetKeys.Facts);
            return Result.Ok(_mapper.Map<FactDTO>(entity));
        }
        else
        {
            const string errorMsg = "Failed to save the fact.";
            return _validator.LogAndFail(request, errorMsg);
        }
    }
}