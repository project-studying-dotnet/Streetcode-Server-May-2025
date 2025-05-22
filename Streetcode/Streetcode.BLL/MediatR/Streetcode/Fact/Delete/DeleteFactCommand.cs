using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Delete
{
    public record DeleteFactCommand(int id) : IRequest<Result<Unit>>;
}
