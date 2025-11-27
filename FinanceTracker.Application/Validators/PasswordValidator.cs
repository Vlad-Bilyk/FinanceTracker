using FinanceTracker.Application.Common;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
                .WithMessage("Password is required")
            .MinimumLength(ValidationConstants.PasswordMinLength)
                .WithMessage($"Password must be at least {ValidationConstants.PasswordMinLength} characters")
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d")
                .WithMessage("Password must contain at least one digit");
    }
}
