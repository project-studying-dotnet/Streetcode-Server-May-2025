using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Partner;
using Streetcode.DAL.Specifications.Streetcode;

namespace Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;

public class GetPartnersByStreetcodeIdHandler : IRequestHandler<GetPartnersByStreetcodeIdQuery, Result<IEnumerable<PartnerDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetPartnersByStreetcodeIdHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PartnerDTO>>> Handle(GetPartnersByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var streetcodeSpec = new StreetcodeByIdSpec(request.StreetcodeId);
        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetBySpecAsync(streetcodeSpec, cancellationToken);

        if (streetcode is null)
        {
            string errorMsg = $"Cannot find any partners with corresponding streetcode id: {request.StreetcodeId}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var partnersSpec = new PartnersByStreetcodeIdSpec(request.StreetcodeId);
        var partners = await _repositoryWrapper.PartnersRepository.ListAsync(partnersSpec, cancellationToken);

        if (partners is null)
        {
            string errorMsg = $"Cannot find partners by a streetcode id: {request.StreetcodeId}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(value: _mapper.Map<IEnumerable<PartnerDTO>>(partners));
    }
}