using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class FinancialOperationUpsertValidator : AbstractValidator<FinancialOperationUpsertDto>
{
    public FinancialOperationUpsertValidator()
    {
        RuleFor(x => x.TypeId)
            .NotEmpty()
                .WithMessage("Operation type is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
                .WithMessage("Amount must be greater than 0")
            .PrecisionScale(18, 2, false)
                .WithMessage("Amount can have maximum 2 decimal places and cannot exceed 18 digits in total");

        RuleFor(x => x.Date)
            .NotEmpty()
                .WithMessage("Operation date is required")
            .LessThanOrEqualTo(DateTimeOffset.Now)
                .WithMessage("Operation date cannot be in the future");

        RuleFor(x => x.Note)
            .MaximumLength(ValidationConstants.OperationNoteMaxLength)
                .WithMessage($"Note cannot exceed {ValidationConstants.OperationNoteMaxLength} characters");
    }
}
