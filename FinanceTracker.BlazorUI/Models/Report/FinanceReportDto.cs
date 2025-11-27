namespace FinanceTracker.BlazorUI.Models.Report;

public class FinanceReportDto
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Net => TotalIncome - TotalExpense;

    public IReadOnlyCollection<CategoryAmountDto> IncomeByCategory { get; init; } = [];
    public IReadOnlyCollection<CategoryAmountDto> ExpensesByCategory { get; init; } = [];
}
