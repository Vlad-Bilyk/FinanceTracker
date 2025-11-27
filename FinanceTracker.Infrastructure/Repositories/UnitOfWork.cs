using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Infrastructure.Data;

namespace FinanceTracker.Infrastructure.Repositories;

/// <summary>
/// Implementation of Unit of Work
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IFinancialOperationTypeRepository FinancialOperationTypes { get; }
    public IFinancialOperationRepository FinancialOperations { get; }
    public IUserRepository Users { get; }
    public IWalletRepository Wallets { get; }
    public ICurrencyRepository Currencies { get; }

    public UnitOfWork(
        AppDbContext context,
        IFinancialOperationTypeRepository typeRepository,
        IFinancialOperationRepository operationRepository,
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        ICurrencyRepository currencyRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        FinancialOperationTypes = typeRepository ?? throw new ArgumentNullException(nameof(typeRepository));
        FinancialOperations = operationRepository ?? throw new ArgumentNullException(nameof(operationRepository));
        Users = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        Wallets = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        Currencies = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
