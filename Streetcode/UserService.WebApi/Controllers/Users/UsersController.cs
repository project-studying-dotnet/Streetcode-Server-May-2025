using AutoMapper;
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

        if (result.IsSuccess)
        {
            var responseDto = _mapper.Map<UserResponseDTO>(result.Value);
            return Ok(responseDto);
        }

        return BadRequest(new { Error = result.Errors.FirstOrDefault()?.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request, 
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);

        return HandleResult(result);
    }
}

