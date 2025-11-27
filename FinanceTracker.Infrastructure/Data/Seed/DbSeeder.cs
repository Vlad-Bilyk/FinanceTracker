using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Interfaces.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Data.Seed;

/// <summary>
/// Seeding a database with initial data
/// </summary>
public class DbSeeder : IDbSeeder
{
    private readonly AppDbContext _context;
    private readonly ICurrencyCatalogProvider _currencyCatalogProvider;
    private readonly IPasswordHasher _passwordHasher;

    // Predefined GUIDs for consistent seeding
    private readonly Guid _adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _regularUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly Guid _adminWallet1Id = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private readonly Guid _adminWallet2Id = Guid.Parse("11111111-1111-1111-1111-111111111113");
    private readonly Guid _userWallet1Id = Guid.Parse("22222222-2222-2222-2222-222222222223");
    private readonly Guid _userWallet2Id = Guid.Parse("22222222-2222-2222-2222-222222222224");

    public DbSeeder(AppDbContext context, ICurrencyCatalogProvider currencyCatalogProvider,
        IPasswordHasher passwordHasher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currencyCatalogProvider = currencyCatalogProvider
            ?? throw new ArgumentNullException(nameof(currencyCatalogProvider));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <inheritdoc/>
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        if (!await _context.Currencies.AnyAsync(ct))
        {
            await AddCurrenciesAsync(ct);
        }
        if (!await _context.Users.AnyAsync(ct))
        {
            await AddUsersAsync(ct);
        }
        if (!await _context.Wallets.AnyAsync(ct))
        {
            await AddWalletsAsync(ct);
        }
        if (!await _context.FinancialOperationTypes.AnyAsync(ct))
        {
            await AddOperationTypesAsync(ct);
        }
        if (!await _context.FinancialOperations.AnyAsync(ct))
        {
            await AddFinancialOperationsAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    private async Task AddCurrenciesAsync(CancellationToken ct)
    {
        var currencies = await _currencyCatalogProvider.GetCurrenciesAsync(ct);

        foreach (var currency in currencies)
        {
            _context.Add(new Currency
            {
                Code = currency.Code,
                Name = currency.Name,
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task AddUsersAsync(CancellationToken ct)
    {
        var users = new List<User>
        {
            new()
            {
                Id = _adminUserId,
                UserName = "admin",
                PasswordHash = _passwordHasher.HashPassword("admin")
            },
            new()
            {
                Id = _regularUserId,
                UserName = "john_doe",
                PasswordHash = _passwordHasher.HashPassword("Password1")
            }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync(ct);
    }

    private async Task AddWalletsAsync(CancellationToken ct)
    {
        var wallets = new List<Wallet>
        {
            // Admin wallets
            new()
            {
                Id = _adminWallet1Id,
                UserId = _adminUserId,
                Name = "Main USD Wallet",
                BaseCurrencyCode = "USD",
            },
            new()
            {
                Id = _adminWallet2Id,
                UserId = _adminUserId,
                Name = "Travel Wallet",
                BaseCurrencyCode = "EUR",
            },
            
            // Regular user wallets
            new()
            {
                Id = _userWallet1Id,
                UserId = _regularUserId,
                Name = "Savings",
                BaseCurrencyCode = "PLN",
            },
            new()
            {
                Id = _userWallet2Id,
                UserId = _regularUserId,
                Name = "Investment",
                BaseCurrencyCode = "JPY",
            }
        };

        _context.Wallets.AddRange(wallets);
        await _context.SaveChangesAsync(ct);
    }

    private async Task AddOperationTypesAsync(CancellationToken ct)
    {
        var operationTypes = new List<FinancialOperationType>
        {
            // Admin operation types (2 types)
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111114"),
                UserId = _adminUserId,
                Name = "Salary",
                Description = "Monthly salary income",
                Kind = OperationKind.Income
            },
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111115"),
                UserId = _adminUserId,
                Name = "Groceries",
                Description = "Food and household items",
                Kind = OperationKind.Expense
            },
            
            // Regular user operation types (1 type)
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222225"),
                UserId = _regularUserId,
                Name = "Freelance",
                Description = "Freelance project income",
                Kind = OperationKind.Income
            }
        };

        _context.FinancialOperationTypes.AddRange(operationTypes);
        await _context.SaveChangesAsync(ct);
    }

    private async Task AddFinancialOperationsAsync(CancellationToken ct)
    {
        var baseDate = new DateTime(2025, 10, 1);

        var operations = new List<FinancialOperation>
        {
            // Admin Wallet 1 (USD) - 5 operations without CurrencyOriginalCode
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111114"), // Salary
                AmountBase = 5000m,
                AmountOriginal = 5000m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(1),
                Note = "January salary",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 150m,
                AmountOriginal = 150m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(2),
                Note = "Weekly shopping",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 85.50m,
                AmountOriginal = 85.50m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(5),
                Note = "Fresh produce",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 200m,
                AmountOriginal = 200m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(10),
                Note = "Monthly groceries",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111114"), // Salary
                AmountBase = 500m,
                AmountOriginal = 500m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(15),
                Note = "Bonus payment",
                IsDeleted = false
            },

            // Admin Wallet 1 (USD) - 2 operations with EUR
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 108m, // 100 EUR * 1.08 rate
                AmountOriginal = 100m,
                CurrencyOriginalCode = "EUR",
                Date = baseDate.AddDays(3),
                Note = "Shopping in Europe",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 54m, // 50 EUR * 1.08 rate
                AmountOriginal = 50m,
                CurrencyOriginalCode = "EUR",
                Date = baseDate.AddDays(7),
                Note = "Restaurant in Paris",
                IsDeleted = false
            },

            // Admin Wallet 1 (USD) - 2 operations with PLN
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 62.5m, // 250 PLN * 0.25 rate
                AmountOriginal = 250m,
                CurrencyOriginalCode = "PLN",
                Date = baseDate.AddDays(12),
                Note = "Shopping in Warsaw",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet1Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 37.5m, // 150 PLN * 0.25 rate
                AmountOriginal = 150m,
                CurrencyOriginalCode = "PLN",
                Date = baseDate.AddDays(14),
                Note = "Polish supermarket",
                IsDeleted = false
            },

            // Admin Wallet 2 (EUR) - 3 operations with JPY
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet2Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 68.49m, // 10000 JPY * 0.006849 rate (approx EUR conversion)
                AmountOriginal = 10000m,
                CurrencyOriginalCode = "JPY",
                Date = baseDate.AddDays(4),
                Note = "Shopping in Tokyo",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet2Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 34.25m, // 5000 JPY * 0.006849 rate
                AmountOriginal = 5000m,
                CurrencyOriginalCode = "JPY",
                Date = baseDate.AddDays(8),
                Note = "Japanese restaurant",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _adminWallet2Id,
                TypeId = Guid.Parse("11111111-1111-1111-1111-111111111115"), // Groceries
                AmountBase = 136.98m, // 20000 JPY * 0.006849 rate
                AmountOriginal = 20000m,
                CurrencyOriginalCode = "JPY",
                Date = baseDate.AddDays(16),
                Note = "Electronics in Japan",
                IsDeleted = false
            },

            // Regular User Wallet 1 (PLN) - 2 operations
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _userWallet1Id,
                TypeId = Guid.Parse("22222222-2222-2222-2222-222222222225"), // Freelance
                AmountBase = 2000m, // In PLN (base currency)
                AmountOriginal = 2000m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(6),
                Note = "Website development project",
                IsDeleted = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                WalletId = _userWallet1Id,
                TypeId = Guid.Parse("22222222-2222-2222-2222-222222222225"), // Freelance
                AmountBase = 1500m, // In PLN (base currency)
                AmountOriginal = 1500m,
                CurrencyOriginalCode = null,
                Date = baseDate.AddDays(20),
                Note = "Logo design",
                IsDeleted = false
            }

            // Regular User Wallet 2 (JPY) - empty (no operations added)
        };

        _context.FinancialOperations.AddRange(operations);
        await _context.SaveChangesAsync(ct);
    }
}
