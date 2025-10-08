using FluentValidation;
using AffiliateSystem.Application.DTOs.User;

namespace AffiliateSystem.Application.Validators;

/// <summary>
/// Validator for create referral link requests
/// </summary>
public class CreateReferralLinkRequestValidator : AbstractValidator<CreateReferralLinkRequest>
{
    public CreateReferralLinkRequestValidator()
    {
        RuleFor(x => x.MaxUsages)
            .GreaterThan(0).WithMessage("Max usages must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Max usages cannot exceed 1000")
            .When(x => x.MaxUsages.HasValue);

        RuleFor(x => x.ExpiresAt)
            .Must(expiresAt => expiresAt > DateTime.UtcNow)
            .WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.CustomCode)
            .MaximumLength(50).WithMessage("Custom code cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Code can only contain letters, numbers, hyphens, and underscores")
            .When(x => !string.IsNullOrEmpty(x.CustomCode));
    }
}
