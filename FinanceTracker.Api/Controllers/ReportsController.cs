using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Route("api/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportService _ReportService;

    public ReportsController(IReportService financeReportService)
    {
        _ReportService = financeReportService ?? throw new ArgumentNullException(nameof(financeReportService));
    }

    /// <summary>
    /// Get daily financial report
    /// </summary>
    /// <param name="date">Date (YYYY-MM-DD)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Daily financial report</returns>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(FinanceReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateOnly date, CancellationToken ct)
    {
        var financeReport = await _ReportService.CreateDailyReportAsync(date, ct);
        return Ok(financeReport);
    }

    /// <summary>
    /// Get period financial report
    /// </summary>
    /// <param name="start">Start date (YYYY-MM-DD)</param>
    /// <param name="end">End date (YYYY-MM-DD)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Period financial report</returns>
    [HttpGet("period")]
    [ProducesResponseType(typeof(FinanceReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPeriodReport([FromQuery] DateOnly start, [FromQuery] DateOnly end, CancellationToken ct)
    {
        var financeReport = await _ReportService.CreatePeriodReportAsync(start, end, ct);
        return Ok(financeReport);
    }
}
