using UserService.WebApi.DTO.Auth.Requests;
using FluentResults;
using UserService.WebApi.MediatR;

namespace UserService.WebApi.MediatR.Auth.ChangePassword;

public record ChangePasswordCommand(ChangePasswordRequestDTO Dto)
    : IRequest<Result>;