using FinanceTracker.Application.Common;
using FinanceTracker.Application.DTOs;
using FluentValidation;

namespace FinanceTracker.Application.Validators;

public class OperationTypeUpdateValidator : AbstractValidator<OperationTypeUpdateDto>
{
    public OperationTypeUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(ValidationConstants.OperationTypeNameMaxLength)
                .WithMessage($"Name cannot exceed {ValidationConstants.OperationTypeNameMaxLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.OperationTypeDescriptionMaxLength)
                .WithMessage($"Description cannot exceed {ValidationConstants.OperationTypeDescriptionMaxLength} characters"); ;
    }
}
