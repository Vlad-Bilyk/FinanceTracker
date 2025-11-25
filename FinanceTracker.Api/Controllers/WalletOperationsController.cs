using FinanceTracker.Application.DTOs.Operation;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Manages financial operations inside a specific wallet of the current user.
/// </summary>
[Route("api/wallets/{walletId:guid}/operations")]
[Authorize]
[ApiController]
public class WalletOperationsController : ControllerBase
{
    private readonly IFinancialOperationService _financialOperationService;

    /// <summary>
    /// Initializes a new instance of <see cref="WalletOperationsController"/>.
    /// </summary>
    /// <param name="financialOperationService">Domain service for wallet operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="financialOperationService"/> is null.</exception>
    public WalletOperationsController(IFinancialOperationService financialOperationService)
    {
        _financialOperationService = financialOperationService ?? throw new ArgumentNullException(nameof(financialOperationService));
    }

    /// <summary>
    /// Gets an operation by identifier
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="id">Operation identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Operation details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FinancialOperationDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FinancialOperationDetailsDto>> GetOperationById(Guid walletId, Guid id, CancellationToken ct)
    {
        var finOperation = await _financialOperationService.GetOperationByIdAsync(walletId, id, ct);
        return Ok(finOperation);
    }

    /// <summary>
    /// Gets all operations for wallet
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of operations.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FinancialOperationDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FinancialOperationDetailsDto>>> GetOperations(Guid walletId, CancellationToken ct)
    {
        var finOperations = await _financialOperationService.GetOperationsForWalletAsync(walletId, ct);
        return Ok(finOperations);
    }

    /// <summary>
    /// Creates a new operation
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="createDto">Creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Location of the created resource.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOperation(Guid walletId,
        FinancialOperationUpsertDto createDto, CancellationToken ct)
    {
        var id = await _financialOperationService.CreateOperationAsync(walletId, createDto, ct);
        return CreatedAtAction(nameof(GetOperationById), new { walletId, id }, new { id });
    }

    /// <summary>
    /// Updates an existing operation
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="id">Operation identifier.</param>
    /// <param name="updateDto">Update payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOperation(Guid walletId, Guid id,
        FinancialOperationUpsertDto updateDto, CancellationToken ct)
    {
        await _financialOperationService.UpdateOperationAsync(walletId, id, updateDto, ct);
        return NoContent();
    }

    /// <summary>
    /// Soft delete an operation
    /// </summary>
    /// <param name="walletId">Wallet identifier.</param>
    /// <param name="id">Operation identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOperation(Guid walletId, Guid id, CancellationToken ct)
    {
        await _financialOperationService.SoftDeleteOperationAsync(walletId, id, ct);
        return NoContent();
    }
}
