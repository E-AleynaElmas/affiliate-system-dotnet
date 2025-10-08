using FluentValidation;
using AffiliateSystem.Application.DTOs.Auth;

namespace AffiliateSystem.Application.Validators;

/// <summary>
/// Validator for login requests
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters");

        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("CAPTCHA verification is required")
            .When(x => !string.IsNullOrEmpty(x.CaptchaToken)); // Only validate if provided
    }
}