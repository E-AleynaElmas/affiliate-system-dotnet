using FluentValidation;
using AffiliateSystem.Application.DTOs.Auth;
using System.Text.RegularExpressions;

namespace AffiliateSystem.Application.Validators;

/// <summary>
/// Validator for registration requests
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character");

        RuleFor(x => x.PasswordConfirm)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.Password).WithMessage("Password confirmation does not match");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters")
            .Matches(@"^[a-zA-ZğüşöçİĞÜŞÖÇ\s]+$").WithMessage("First name can only contain letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters")
            .Matches(@"^[a-zA-ZğüşöçİĞÜŞÖÇ\s]+$").WithMessage("Last name can only contain letters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Please provide a valid phone number")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.ReferralCode)
            .Length(8).WithMessage("Invalid referral code format")
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Referral code can only contain letters and numbers")
            .When(x => !string.IsNullOrEmpty(x.ReferralCode));

        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("CAPTCHA verification is required");
    }
}