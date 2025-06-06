﻿using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Partners.Update;

public class UpdatePartnerHandler : IRequestHandler<UpdatePartnerCommand, Result<PartnerDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public UpdatePartnerHandler(
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

    public async Task<Result<PartnerDTO>> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = _mapper.Map<Partner>(request.Partner);

        try
        {
            var links = await _repositoryWrapper.PartnerSourceLinkRepository
               .GetAllAsync(predicate: l => l.PartnerId == partner.Id);

            var newLinkIds = partner.PartnerSourceLinks.Select(l => l.Id).ToList();

            foreach (var link in links)
            {
                if (!newLinkIds.Contains(link.Id))
                {
                    _repositoryWrapper.PartnerSourceLinkRepository.Delete(link);
                }
            }

            partner.Streetcodes.Clear();
            _repositoryWrapper.PartnersRepository.Update(partner);
            await _repositoryWrapper.SaveChangesAsync();
            var newStreetcodeIds = request.Partner.Streetcodes.Select(s => s.Id).ToList();
            var oldStreetcodes = await _repositoryWrapper.PartnerStreetcodeRepository
                .GetAllAsync(ps => ps.PartnerId == partner.Id);

            foreach (var old in oldStreetcodes!)
            {
                if (!newStreetcodeIds.Contains(old.StreetcodeId))
                {
                    _repositoryWrapper.PartnerStreetcodeRepository.Delete(old);
                }
            }

            foreach (var newCodeId in newStreetcodeIds!)
            {
                if (oldStreetcodes.FirstOrDefault(x => x.StreetcodeId == newCodeId) == null)
                {
                    await _repositoryWrapper.PartnerStreetcodeRepository.CreateAsync(
                        new StreetcodePartner() { PartnerId = partner.Id, StreetcodeId = newCodeId });
                }
            }

            await _repositoryWrapper.SaveChangesAsync();
            var dbo = _mapper.Map<PartnerDTO>(partner);
            dbo.Streetcodes = request.Partner.Streetcodes;

            await _cacheInvalidationService.InvalidateCacheAsync(Constants.CacheSetKeys.Partners);
            
            return Result.Ok(dbo);
        }
        catch (Exception ex)
        {
            _logger.LogError(request, ex.Message);
            return Result.Fail(ex.Message);
        }
    }
}