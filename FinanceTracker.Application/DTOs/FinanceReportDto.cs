using FinanceTracker.Application.DTOs.Operation;

namespace FinanceTracker.Application.DTOs;

public class FinanceReportDto
{
    public Guid WalletId { get; init; }
    public string WalletName { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public DateOnly Start { get; init; }
    public DateOnly End { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Net => TotalIncome - TotalExpense;
    public IReadOnlyList<FinancialOperationDetailsDto> Operations { get; init; } = [];
}
