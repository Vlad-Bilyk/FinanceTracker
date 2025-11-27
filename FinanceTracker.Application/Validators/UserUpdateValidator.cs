using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs.User;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(ValidationConstants.UserNameMaxLength)
                .WithMessage($"Username cannot exceed {ValidationConstants.UserNameMaxLength} characters\"");
    }
}
