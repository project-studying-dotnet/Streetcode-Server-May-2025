using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update;

public record UpdateFactsCommand(FactUpdateCreateDTO FactUpdateCreateDto) : IRequest<Result<FactDTO>>;