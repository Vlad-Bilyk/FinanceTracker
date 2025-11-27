namespace FinanceTracker.Application.DTOs.Report;

/// <summary>
/// Represents aggregated amount for a single operation type.
/// </summary>
public class CategoryAmountDto
{
    /// <summary>
    /// Operation type identifier.
    /// </summary>
    public Guid TypeId { get; set; }

    /// <summary>
    /// Operation type name.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Total amount for this type in the report.
    /// </summary>
    public decimal Amount { get; set; }
}
