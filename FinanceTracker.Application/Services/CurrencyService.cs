using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Exceptions;
using FinanceTracker.Application.Interfaces.Repositories;
using FinanceTracker.Application.Interfaces.Services;

namespace FinanceTracker.Application.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IReadOnlyList<CurrencyDto>> GetAllCurrenciesAsync(CancellationToken ct = default)
    {
        var currencies = await _unitOfWork.Currencies.GetAllAsync(ct);

        return currencies.Select(c => new CurrencyDto(c.Code, c.Name)).ToList();
    }

    public async Task<CurrencyDto> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var normalizedCode = code.ToUpperInvariant();

        var currency = await _unitOfWork.Currencies.GetByCodeAsync(normalizedCode, ct)
            ?? throw new NotFoundException($"Currency with code '{code}' was not found.");

        return new CurrencyDto(currency.Code, currency.Name);
    }
}
