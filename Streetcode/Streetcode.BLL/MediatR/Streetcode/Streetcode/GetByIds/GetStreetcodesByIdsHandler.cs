using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByIds;

public class GetStreetcodesByIdsHandler : IRequestHandler<GetStreetcodesByIdsQuery, Result<IEnumerable<StreetcodeDTO>>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public GetStreetcodesByIdsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<StreetcodeDTO>>> Handle(GetStreetcodesByIdsQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids is null || !request.Ids.Any())
        {
            return Result.Ok(Enumerable.Empty<StreetcodeDTO>());
        }

        var streetcodes = await _repositoryWrapper.StreetcodeRepository
            .GetAllAsync();

        if (streetcodes is null || !streetcodes.Any())
        {
            string errorMsg = "Cannot find any streetcodes by provided ids.";
            _logger.LogError(request, errorMsg);
            return Result.Fail<IEnumerable<StreetcodeDTO>>(errorMsg);
        }

        var streetcodeDtos = _mapper.Map<IEnumerable<StreetcodeDTO>>(streetcodes);
        return Result.Ok(streetcodeDtos);
    }
}
