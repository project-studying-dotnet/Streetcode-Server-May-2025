using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Partners.Create;

public class CreatePartnerHandler : IRequestHandler<CreatePartnerCommand, Result<PartnerDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public CreatePartnerHandler(
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

    public async Task<Result<PartnerDTO>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var newPartner = _mapper.Map<Partner>(request.newPartner);
        try
        {
            newPartner.Streetcodes.Clear();
            newPartner = await _repositoryWrapper.PartnersRepository.CreateAsync(newPartner);
            await _repositoryWrapper.SaveChangesAsync();
            var streetcodeIds = request.newPartner.Streetcodes.Select(s => s.Id).ToList();
            if (streetcodeIds.Any())
            {
                var streetcodes = await _repositoryWrapper
                    .StreetcodeRepository
                    .GetAllAsync(s => streetcodeIds.Contains(s.Id));
                newPartner.Streetcodes.AddRange(streetcodes);
            }

            await _repositoryWrapper.SaveChangesAsync();
            await _cacheInvalidationService.InvalidateCacheAsync(Constants.CacheSetKeys.Partners);
            return Result.Ok(_mapper.Map<PartnerDTO>(newPartner));
        }
        catch (Exception ex)
        {
            _logger.LogError(request, ex.Message);
            return Result.Fail(ex.Message);
        }
    }
}