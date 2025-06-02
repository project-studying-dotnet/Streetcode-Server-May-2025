using Microsoft.AspNetCore.Mvc;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Controllers.Users;

[ApiController]
[Route("/api/auth")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDTO, CancellationToken cancellationToken)
    {
        var newAccount = await _authService.Register(registerUserDTO, cancellationToken);

        return Ok(newAccount.Value);
    }
}

