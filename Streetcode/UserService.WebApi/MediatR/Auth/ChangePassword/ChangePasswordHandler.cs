using FluentResults;
using UserService.WebApi.MediatR;
using UserService.WebApi.Services.Interfaces;

namespace UserService.WebApi.MediatR.Auth.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IAuthService _authService;

    public ChangePasswordHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return await _authService.ChangePasswordAsync(request.Dto, cancellationToken);
    }
}