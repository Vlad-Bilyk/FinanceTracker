using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.BlazorUI.Models.Operation;

public class OperationFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Operation type is required.")]
    public Guid TypeId { get; set; }

    [Required(ErrorMessage = "Wallet is required.")]
    public Guid WalletId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999",
        ErrorMessage = "Amount must be greater than zero.")]
    public decimal AmountOriginal { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    [DataType(DataType.Currency)]
    public string CurrencyOriginalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required.")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; } = DateTime.Now;

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string? Note { get; set; }
}
