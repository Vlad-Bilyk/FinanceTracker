using FinanceTracker.BlazorUI.Models.Commons;
using System.ComponentModel.DataAnnotations;
namespace FinanceTracker.BlazorUI.Models.OperationType;

public sealed class OperationTypeFormModel
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Required]
    public OperationKind Kind { get; set; }
}
