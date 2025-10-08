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
        RuleFor(x => x.Email).EmailRules();

        RuleFor(x => x.Password).PasswordRules();

        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("CAPTCHA verification is required")
            .When(x => !string.IsNullOrEmpty(x.CaptchaToken)); // Only validate if provided
    }
}