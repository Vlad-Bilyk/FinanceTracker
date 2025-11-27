using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs.OperationType;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class OperationTypeCreateValidator : AbstractValidator<OperationTypeCreateDto>
{
    public OperationTypeCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(ValidationConstants.OperationTypeNameMaxLength)
                .WithMessage($"Name cannot exceed {ValidationConstants.OperationTypeNameMaxLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.OperationTypeDescriptionMaxLength)
                .WithMessage($"Description cannot exceed {ValidationConstants.OperationTypeDescriptionMaxLength} characters");

        RuleFor(x => x.Kind)
            .IsInEnum().WithMessage("Invalid operation kind. Allowed values: Income, Expense");
    }
}
