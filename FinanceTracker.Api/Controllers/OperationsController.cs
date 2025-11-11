using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Route("api/operations")]
[ApiController]
public class OperationsController : ControllerBase
{
    private readonly IFinancialOperationService _financialOperationService;

    public OperationsController(IFinancialOperationService financialOperationService)
    {
        _financialOperationService = financialOperationService ?? throw new ArgumentNullException(nameof(financialOperationService));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FinancialOperationDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FinancialOperationDetailsDto>> GetOperationById(Guid id, CancellationToken ct)
    {
        var finOperation = await _financialOperationService.GetOperationByIdAsync(id, ct);
        return Ok(finOperation);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FinancialOperationDetailsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FinancialOperationDetailsDto>>> GetOperations(CancellationToken ct)
    {
        var finOperations = await _financialOperationService.GetAllOperationsAsync(ct);
        return Ok(finOperations);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOperation(FinancialOperationUpsertDto createDto, CancellationToken ct)
    {
        var id = await _financialOperationService.CreateOperationAsync(createDto, ct);
        return CreatedAtAction(nameof(GetOperationById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOperation(Guid id, FinancialOperationUpsertDto updateDto, CancellationToken ct)
    {
        await _financialOperationService.UpdateOperationAsync(id, updateDto, ct);
        return NoContent();
    }

    /// <summary>
    /// Soft delete
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOperation(Guid id, CancellationToken ct)
    {
        await _financialOperationService.SoftDeleteOperationAsync(id, ct);
        return NoContent();
    }
}
