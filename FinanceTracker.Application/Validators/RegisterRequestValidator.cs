using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs.Auth;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(ValidationConstants.UserNameMaxLength)
                .WithMessage($"Username cannot exceed {ValidationConstants.UserNameMaxLength} characters\"");

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator());
    }
}
