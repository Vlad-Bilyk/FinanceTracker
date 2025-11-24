using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.BlazorUI.Models.Wallet;

public class WalletFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Base currency code is required")]
    [DataType(DataType.Currency)]
    public string BaseCurrencyCode { get; set; } = string.Empty;
}
