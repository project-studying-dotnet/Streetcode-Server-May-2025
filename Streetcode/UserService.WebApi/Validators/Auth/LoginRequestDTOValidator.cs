﻿using FluentValidation;
using UserService.WebApi.DTO.Auth.Requests;

namespace UserService.WebApi.Validators.Auth;

public class LoginRequestDTOValidator : AbstractValidator<LoginRequestDTO>
{
    public LoginRequestDTOValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");
    }
}