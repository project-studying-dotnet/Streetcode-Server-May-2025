using AutoMapper;
using FluentResults;
using UserService.WebApi.Data.Repositories.Interfaces;
using UserService.WebApi.DTO.Users;
using UserService.WebApi.Entities.Users;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.Services.Realisations;
public class AuthService : IAuthService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsersRepository usersRepository, 
        IPasswordHasher passwordHasher, 
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<Result<RegisterUserDTO>> Register(RegisterUserDTO registerUserDTO, CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordHasher.Generate(registerUserDTO.Password);

        var newUser = _mapper.Map<User>(registerUserDTO);

        await _usersRepository.AddAsync(newUser, cancellationToken);

        var resultIsSuccess = await _usersRepository.SaveChangesAsync() > 0;

        if (!resultIsSuccess)
        {
            const string errorMsg = "Failed to create a user";
            _logger.LogError(errorMsg);
            return Result.Fail<RegisterUserDTO>(errorMsg);
        }

        _logger.LogInformation("Created user with id {UserId}", newUser.Id);
        return Result.Ok(_mapper.Map<RegisterUserDTO>(newUser));
    }
}

