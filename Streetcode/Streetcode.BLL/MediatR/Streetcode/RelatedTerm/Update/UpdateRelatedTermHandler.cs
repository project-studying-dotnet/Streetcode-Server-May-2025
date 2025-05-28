using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;

public class UpdateRelatedTermHandler : IRequestHandler<UpdateRelatedTermCommand, Result<Unit>>
{
    public UpdateRelatedTermHandler(IMapper mapper, IRepositoryWrapper repository) { }

    public Task<Result<Unit>> Handle(UpdateRelatedTermCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}