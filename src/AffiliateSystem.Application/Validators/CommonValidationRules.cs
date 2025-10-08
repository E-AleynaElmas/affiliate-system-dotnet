using FluentValidation;

namespace AffiliateSystem.Application.Validators;

/// <summary>
/// Common validation rules for reusable validation logic
/// </summary>
public static class CommonValidationRules
{
    /// <summary>
    /// Standard email validation rules
    /// </summary>
    public static IRuleBuilderOptions<T, string> EmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
    }

    /// <summary>
    /// Standard password validation rules
    /// </summary>
    public static IRuleBuilderOptions<T, string> PasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        bool requireComplexity = false)
    {
        var builder = ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters");

        if (requireComplexity)
        {
            builder.Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])")
                .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character");
        }

        return builder;
    }

    /// <summary>
    /// Standard string length validation
    /// </summary>
    public static IRuleBuilderOptions<T, string> StandardStringLength<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int minLength = 2,
        int maxLength = 100)
    {
        return ruleBuilder
            .MinimumLength(minLength).WithMessage($"Must be at least {minLength} characters long")
            .MaximumLength(maxLength).WithMessage($"Must not exceed {maxLength} characters");
    }

    /// <summary>
    /// Phone number validation (optional)
    /// </summary>
    public static IRuleBuilderOptions<T, string?> PhoneNumberRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Please provide a valid phone number")
            .When(x => !string.IsNullOrEmpty(ruleBuilder.ToString()));
    }
}
