namespace FinanceTracker.Application.Interfaces.Repositories;

/// <summary>
/// Defines a unit of work that coordinates the work of multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    IFinancialOperationTypeRepository FinancialOperationTypes { get; }
    IFinancialOperationRepository FinancialOperations { get; }
    IUserRepository Users { get; }
    IWalletRepository Wallets { get; }
    ICurrencyRepository Currencies { get; }

    /// <summary>
    /// Save all tracked changes as a single unit.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

