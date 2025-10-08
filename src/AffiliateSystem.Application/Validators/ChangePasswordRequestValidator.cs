using FluentValidation;
using AffiliateSystem.Application.DTOs.User;

namespace AffiliateSystem.Application.Validators;

/// <summary>
/// Validator for change password requests
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required")
            .MinimumLength(1).WithMessage("Current password cannot be empty");

        RuleFor(x => x.NewPassword)
            .PasswordRules(requireComplexity: true);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");

        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword)
            .WithMessage("New password must be different from current password")
            .When(x => !string.IsNullOrEmpty(x.CurrentPassword) && !string.IsNullOrEmpty(x.NewPassword));
    }
}
