using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Reorder;

public record ReorderFactsCommand(IEnumerable<FactReorderDTO> FactReorderDtos, int StreetcodeId) : IRequest<Result<Unit>>;