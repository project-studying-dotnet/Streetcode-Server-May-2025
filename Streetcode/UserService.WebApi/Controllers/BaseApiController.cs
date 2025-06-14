﻿using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace UserService.WebApi.Controllers;

[Route("api/[controller]/[action]")]
public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value == null
                ? NotFound()
                : Ok(result.Value);
        }

        return BadRequest(new
        {
            Errors = result.Errors.Select(e => e.Message)
        });
    }
}