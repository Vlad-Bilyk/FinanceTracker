using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Interfaces;

/// <summary>
/// Provides access to an external currency catalog.
/// </summary>
public interface ICurrencyCatalogProvider
{
    /// <summary>
    /// Retrieves the list of available currencies from an external provider.
    /// </summary>
    /// <param name="ct">>Cancellation token</param>
    /// <returns>
    /// A read-only list of <see cref="CurrencyDto"/> with currency codes and names.
    /// </returns>
    Task<IReadOnlyList<CurrencyDto>> GetCurrenciesAsync(CancellationToken ct = default);
}
