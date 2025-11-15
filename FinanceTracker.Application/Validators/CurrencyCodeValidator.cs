using FinanceTracker.Application.Common;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class CurrencyCodeValidator : AbstractValidator<string?>
{
    public CurrencyCodeValidator()
    {
        RuleFor(code => code)
            .NotEmpty()
                .WithMessage("Currency code is required")
            .Length(ValidationConstants.CurrencyCodeLength)
                .WithMessage("Currency code must be exactly 3 characters long")
            .Matches("^[A-Z]{3}$")
                .WithMessage("Currency code must consist of uppercase letters only");
    }
}
