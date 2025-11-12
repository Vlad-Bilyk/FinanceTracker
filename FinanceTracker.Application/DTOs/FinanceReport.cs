namespace FinanceTracker.Application.DTOs;

public class FinanceReport
{
    public DateOnly Start { get; init; }
    public DateOnly End { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Net => TotalIncome - TotalExpense;
    public IReadOnlyList<FinancialOperationDetailsDto> Operations { get; init; } = [];
}
