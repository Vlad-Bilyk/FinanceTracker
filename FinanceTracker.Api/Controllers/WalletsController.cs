using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Route("api/wallets")]
[ApiController]
public class WalletsController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> GetAllWallets(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetWalletById(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<IActionResult> CreateWallet(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> UpdateWallet(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}")]
    public Task<IActionResult> DeleteWallet(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
