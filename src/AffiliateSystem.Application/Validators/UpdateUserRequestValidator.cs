using FluentValidation;
using AffiliateSystem.Application.DTOs.User;

namespace AffiliateSystem.Application.Validators;

/// <summary>
/// Validator for update user profile requests
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ\\s]+$")
            .WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ\\s]+$")
            .WithMessage("Last name can only contain letters and spaces");

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber).PhoneNumberRules();
        });
    }
}
