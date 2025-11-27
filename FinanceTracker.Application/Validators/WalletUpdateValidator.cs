using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs.Wallet;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class WalletUpdateValidator : AbstractValidator<WalletUpdateDto>
{
    public WalletUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Wallet name is required.")
            .MaximumLength(ValidationConstants.WalletNameMaxLength)
                .WithMessage($"Wallet name must not exceed {ValidationConstants.WalletNameMaxLength} characters.");
    }
}
