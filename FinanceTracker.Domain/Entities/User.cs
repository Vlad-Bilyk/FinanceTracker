namespace FinanceTracker.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<Wallet> Wallets { get; set; } = [];
    public ICollection<FinancialOperationType> FinancialOperationTypes { get; set; } = [];
}
