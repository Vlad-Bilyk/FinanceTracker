namespace FinanceTracker.BlazorUI.Models.Operation;

public class OperationUpsertDto
{
    public Guid TypeId { get; set; }

    public decimal AmountOriginal { get; set; }
    public string? CurrencyOriginalCode { get; set; }

    public DateTime Date { get; set; }
    public string? Note { get; set; }
}
