using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs.Operation;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class FinancialOperationUpsertValidator : AbstractValidator<FinancialOperationUpsertDto>
{
    public FinancialOperationUpsertValidator()
    {
        RuleFor(x => x.TypeId)
            .NotEmpty()
                .WithMessage("Operation type is required");

        RuleFor(x => x.AmountOriginal)
            .GreaterThan(0)
                .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, false)
                .WithMessage("Amount can have maximum 2 decimal places and cannot exceed 18 digits in total");

        RuleFor(x => x.CurrencyOriginalCode)
            .SetValidator(new CurrencyCodeValidator())
            .When(x => !string.IsNullOrEmpty(x.CurrencyOriginalCode));


        RuleFor(x => x.Date)
            .NotEmpty()
                .WithMessage("Operation date is required")
            .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Operation date cannot be in the future");

        RuleFor(x => x.Note)
            .MaximumLength(ValidationConstants.OperationNoteMaxLength)
                .WithMessage($"Note cannot exceed {ValidationConstants.OperationNoteMaxLength} characters");
    }
}
