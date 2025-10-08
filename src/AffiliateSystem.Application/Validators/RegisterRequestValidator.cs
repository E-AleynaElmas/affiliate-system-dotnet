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
        RuleFor(x => x.Email).EmailRules();

        RuleFor(x => x.Password).PasswordRules(requireComplexity: true);

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

        RuleFor(x => x.PhoneNumber).PhoneNumberRules();

        RuleFor(x => x.ReferralCode)
            .Length(8).WithMessage("Invalid referral code format")
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("Referral code can only contain letters and numbers")
            .When(x => !string.IsNullOrEmpty(x.ReferralCode));

        RuleFor(x => x.CaptchaToken)
            .NotEmpty().WithMessage("CAPTCHA verification is required");
    }
}