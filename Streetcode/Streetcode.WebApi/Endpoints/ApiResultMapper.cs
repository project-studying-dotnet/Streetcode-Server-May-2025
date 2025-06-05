using FluentResults;
using Streetcode.BLL.MediatR.ResultVariations;

using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Streetcode.WebApi.Endpoints;

public static class ApiResultMapper
{
    public static IResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result is NullResult<T>)
            {
                return HttpResults.Ok(result.Value);
            }

            if (result.Value is null)
            {
                return HttpResults.NotFound("Found result matching null");
            }

            return HttpResults.Ok(result.Value);
        }

        return HttpResults.BadRequest(result.Reasons);
    }
}