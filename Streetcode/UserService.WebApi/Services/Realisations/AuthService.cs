using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserService.WebApi.Services.Realisations;
public class AuthService : IAuthService
{
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly UserManager<User> _userManager;

    public AuthService(
        IMapper mapper,
        ILogger<AuthService> logger,
        UserManager<User> userManager)
    {
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<Result<User>> Register(RegisterUserDTO registerUserDTO, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerUserDTO.Email);

        if (existingUser != null)
        {
            return Result.Fail<User>("User with this email already exists");
        }

        var newUser = _mapper.Map<User>(registerUserDTO);
        newUser.UserName = registerUserDTO.Email;

        var result = await _userManager.CreateAsync(newUser, registerUserDTO.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to create user: {errors}");
            return Result.Fail<User>($"Failed to create user: {errors}");
        }

        _logger.LogInformation("Created user with id {UserId}", newUser.Id);
        return Result.Ok(newUser);
    }
}

