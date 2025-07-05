using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByIds;

public class GetStreetcodesByIdsQuery : IRequest<Result<IEnumerable<StreetcodeDTO>>>
{
    public GetStreetcodesByIdsQuery(IEnumerable<int> ids)
    {
        Ids = ids;
    }

    public IEnumerable<int> Ids { get; }
}