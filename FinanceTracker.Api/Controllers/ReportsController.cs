using FinanceTracker.Application.DTOs.Report;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Provides financial reports for a specific wallet of the current user.
/// </summary>
[Route("api/wallets/{walletId:guid}/reports")]
[Authorize]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    /// <summary>
    /// Initializes a new instance of <see cref="ReportsController"/>.
    /// </summary>
    /// <param name="financeReportService">Domain service for building reports.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="financeReportService"/> is null.</exception>
    public ReportsController(IReportService financeReportService)
    {
        _reportService = financeReportService ?? throw new ArgumentNullException(nameof(financeReportService));
    }

    /// <summary>
    /// Get daily financial report
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="date">Date (YYYY-MM-DD)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Daily financial report</returns>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(FinanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDailyReport(
        Guid walletId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var financeReport = await _reportService.CreateDailyReportAsync(walletId, date, ct);
        return Ok(financeReport);
    }

    /// <summary>
    /// Get period financial report
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="start">Start date (YYYY-MM-DD)</param>
    /// <param name="end">End date (YYYY-MM-DD)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Period financial report</returns>
    [HttpGet("period")]
    [ProducesResponseType(typeof(FinanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPeriodReport(
        Guid walletId, [FromQuery] DateOnly start, [FromQuery] DateOnly end, CancellationToken ct)
    {
        var financeReport = await _reportService.CreatePeriodReportAsync(walletId, start, end, ct);
        return Ok(financeReport);
    }
}
