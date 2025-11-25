using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.DTOs.Operation;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Provides access to financial operations of the current user.
/// </summary>
[Route("api/operations")]
[Authorize]
[ApiController]
public class OperationsController : ControllerBase
{
    private readonly IFinancialOperationService _financialOperationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationsController"/> class.
    /// </summary>
    /// <param name="financialOperationService">Service for working with financial operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="financialOperationService"/> is null.</exception>
    public OperationsController(IFinancialOperationService financialOperationService)
    {
        _financialOperationService = financialOperationService ?? throw new ArgumentNullException(nameof(financialOperationService));
    }

    /// <summary>
    /// Retrieves paged financial operations of the current user.
    /// </summary>
    /// <param name="walletId">Optional wallet identifier.</param>
    /// <param name="from">Optional start date (inclusive).</param>
    /// <param name="to">Optional end date (inclusive).</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of financial operations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FinancialOperationDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<FinancialOperationDetailsDto>>> GetUserOperations(
        [FromQuery] Guid? walletId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new OperationQuery
        {
            WalletId = walletId,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };

        var result = await _financialOperationService.GetUserOperationsAsync(query, ct);
        return Ok(result);
    }
}
