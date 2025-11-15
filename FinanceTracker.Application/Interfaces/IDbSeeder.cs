namespace FinanceTracker.Application.Interfaces;

/// <summary>
/// Defines a contract for seeding a database with initial data.
/// </summary>
public interface IDbSeeder
{
    /// <summary>
    /// Seeds the database with initial data if it is empty.
    /// </summary>
    Task SeedAsync(CancellationToken ct = default);
}

