using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create;

public record CreateTextCommand(TextCreateDTO CreateTextRequest) : IRequest<Result<TextDTO>>;