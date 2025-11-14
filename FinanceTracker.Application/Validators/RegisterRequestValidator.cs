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
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one digit");
    }
}
