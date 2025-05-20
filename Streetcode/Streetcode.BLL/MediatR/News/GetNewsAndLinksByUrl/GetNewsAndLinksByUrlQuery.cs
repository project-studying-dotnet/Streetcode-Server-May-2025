﻿using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.GetNewsAndLinksByUrl
{
    public record GetNewsAndLinksByUrlQuery(string url) : IRequest<Result<NewsDTOWithURLs>>;
}
