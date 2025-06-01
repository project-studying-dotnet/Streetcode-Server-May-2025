using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Cache;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Partners.Delete;

public class DeletePartnerHandler : IRequestHandler<DeletePartnerCommand, Result<PartnerDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public DeletePartnerHandler(
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

    public async Task<Result<PartnerDTO>> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await _repositoryWrapper.PartnersRepository.GetFirstOrDefaultAsync(p => p.Id == request.id);
        if (partner == null)
        {
            const string errorMsg = "No partner with such id";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }
        else
        {
            _repositoryWrapper.PartnersRepository.Delete(partner);
            try
            {
                await _repositoryWrapper.SaveChangesAsync();
                await _cacheInvalidationService.InvalidateAllCacheAsync(Constants.CacheSetKeys.Partners);
                return Result.Ok(_mapper.Map<PartnerDTO>(partner));
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }
    }
}