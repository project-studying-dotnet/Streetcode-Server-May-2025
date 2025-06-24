using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.WebApi.DTO.Auth.Requests;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Controllers.Users;

public class UsersController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public UsersController(IAuthService authService, IMapper mapper)
    {
        _authService = authService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDTO, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.Register(registerUserDTO, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request, 
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request, CancellationToken cancellationToken)
    {
        var email = User.Identity?.Name;

        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(request.Email, request.OldPassword, request.NewPassword, cancellationToken);

        return HandleResult(result);
    }
}

