using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Route("api/types")]
[ApiController]
public class OperationTypesController : ControllerBase
{
    private readonly IOperationTypeService _operationTypeService;

    public OperationTypesController(IOperationTypeService operationTypeService)
    {
        _operationTypeService = operationTypeService ?? throw new ArgumentNullException(nameof(operationTypeService));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OperationTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationTypeDto>> GetTypeById(Guid id, CancellationToken ct)
    {
        var operationType = await _operationTypeService.GetTypeByIdAsync(id, ct);
        return Ok(operationType);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OperationTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OperationTypeDto>>> GetTypes(CancellationToken ct)
    {
        var operationTypes = await _operationTypeService.GetAllTypesAsync(ct);
        return Ok(operationTypes);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateType(OperationTypeCreateDto createDto, CancellationToken ct)
    {
        var id = await _operationTypeService.CreateTypeAsync(createDto, ct);
        return CreatedAtAction(nameof(GetTypeById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateType(Guid id, OperationTypeUpdateDto updateDto, CancellationToken ct)
    {
        await _operationTypeService.UpdateTypeAsync(id, updateDto, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteType(Guid id, CancellationToken ct)
    {
        await _operationTypeService.DeleteTypeAsync(id, ct);
        return NoContent();
    }
}
