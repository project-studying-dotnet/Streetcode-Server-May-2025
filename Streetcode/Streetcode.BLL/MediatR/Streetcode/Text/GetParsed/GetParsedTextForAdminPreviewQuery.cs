using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetParsed
{
    public record GetParsedTextForAdminPreviewQuery(string textToParse) : IRequest<Result<string>>
    {
    }
}
