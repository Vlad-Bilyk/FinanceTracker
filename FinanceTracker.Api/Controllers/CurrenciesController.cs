using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Route("api/currencies")]
[ApiController]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
    }

    /// <summary>
    /// Get all available currencies
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CurrencyDto>>> GetAll(CancellationToken ct)
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync(ct);
        return Ok(currencies);
    }

    /// <summary>
    /// Get currency by code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyDto>> GetByCode(string code, CancellationToken ct)
    {
        var currency = await _currencyService.GetByCodeAsync(code, ct);
        return Ok(currency);
    }
}
