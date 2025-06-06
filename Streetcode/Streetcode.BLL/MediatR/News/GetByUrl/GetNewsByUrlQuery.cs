﻿using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.MediatR.News.GetByUrl;

public record GetNewsByUrlQuery(string url) : IRequest<Result<NewsDTO>>;