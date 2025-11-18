using FinanceTracker.Application.DTOs.Wallet;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Manages wallets of the current authenticated user.
/// </summary>
[Route("api/wallets")]
[ApiController]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    /// <summary>
    /// Creates a new <see cref="WalletsController"/>.
    /// </summary>
    /// <param name="walletService">Domain service for wallet operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="walletService"/> is null.</exception>
    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
    }

    /// <summary>
    /// Gets the list of wallets belonging to the current user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of user wallets.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WalletDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<WalletDto>>> GetUserWallets(CancellationToken ct)
    {
        var wallets = await _walletService.GetUserWalletsAsync(ct);
        return Ok(wallets);
    }

    /// <summary>
    /// Gets a wallet by its identifier
    /// </summary>
    /// <param name="id">Wallet identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Wallet details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WalletDto>> GetWalletById(Guid id, CancellationToken ct)
    {
        var wallet = await _walletService.GetWalletByIdAsync(id, ct);
        return Ok(wallet);
    }

    /// <summary>
    /// Creates a new wallet
    /// </summary>
    /// <param name="createDto">Creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Location of the created wallet.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateWallet(WalletCreateDto createDto, CancellationToken ct)
    {
        var id = await _walletService.CreateWalletAsync(createDto, ct);
        return CreatedAtAction(nameof(GetWalletById), new { id }, new { id });
    }

    /// <summary>
    /// Updates an existing wallet
    /// </summary>
    /// <param name="id">Wallet identifier.</param>
    /// <param name="updateDto"></param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateWallet(Guid id, WalletUpdateDto updateDto, CancellationToken ct)
    {
        await _walletService.UpdateWalletAsync(id, updateDto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a wallet (soft delete)
    /// </summary>
    /// <param name="id">Wallet identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWallet(Guid id, CancellationToken ct)
    {
        await _walletService.DeleteWalletAsync(id, ct);
        return NoContent();
    }
}
