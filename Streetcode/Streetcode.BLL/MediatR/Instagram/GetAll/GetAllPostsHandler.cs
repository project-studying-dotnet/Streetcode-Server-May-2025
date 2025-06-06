﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Instagram;
using Streetcode.DAL.Entities.Instagram;

namespace Streetcode.BLL.MediatR.Instagram.GetAll;

public class GetAllPostsHandler : IRequestHandler<GetAllPostsQuery, Result<IEnumerable<InstagramPost>>>
{
    private readonly IInstagramService _instagramService;

    public GetAllPostsHandler(IInstagramService instagramService)
    {
        _instagramService = instagramService;
    }

    public async Task<Result<IEnumerable<InstagramPost>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var result = await _instagramService.GetPostsAsync();
        return Result.Ok(result);
    }
}