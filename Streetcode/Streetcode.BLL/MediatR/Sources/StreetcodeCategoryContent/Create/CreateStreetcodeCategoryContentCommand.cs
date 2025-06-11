using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

public record CreateStreetcodeCategoryContentCommand(CategoryContentCreateDTO CategoryContentDto) : IRequest<Result<StreetcodeCategoryContentDTO>>;