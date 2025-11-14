using FinanceTracker.Application.DTOs.User;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.NewPassword)
            .SetValidator(new PasswordValidator());
    }
}
