using FinanceTracker.Application.DTOs.OperationType;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Manages operation types for the current authenticated user.
/// </summary>
[Route("api/users/{userId:guid}/types")]
[Authorize]
[ApiController]
public class OperationTypesController : ControllerBase
{
    private readonly IOperationTypeService _operationTypeService;

    /// <summary>
    ///  Initializes a new instance of <see cref="OperationTypesController"/>.
    /// </summary>
    /// <param name="operationTypeService">Domain service for operation types.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="operationTypeService"/> is null.</exception>
    public OperationTypesController(IOperationTypeService operationTypeService)
    {
        _operationTypeService = operationTypeService ?? throw new ArgumentNullException(nameof(operationTypeService));
    }

    /// <summary>
    /// Gets an operation type by identifier
    /// </summary>
    /// <param name="id">Operation type identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Operation type details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OperationTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationTypeDto>> GetTypeById(Guid id, CancellationToken ct)
    {
        var operationType = await _operationTypeService.GetTypeByIdAsync(id, ct);
        return Ok(operationType);
    }

    /// <summary>
    /// Gets all operation types 
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of operation types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OperationTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OperationTypeDto>>> GetUserTypes(CancellationToken ct)
    {
        var operationTypes = await _operationTypeService.GetUserTypesAsync(ct);
        return Ok(operationTypes);
    }

    /// <summary>
    /// Creates a new operation type
    /// </summary>
    /// <param name="createDto"></param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Location of the created resource.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateType(OperationTypeCreateDto createDto, CancellationToken ct)
    {
        var id = await _operationTypeService.CreateTypeAsync(createDto, ct);
        return CreatedAtAction(nameof(GetTypeById), new { id }, new { id });
    }

    /// <summary>
    /// Updates an existing operation type
    /// </summary>
    /// <param name="id">Operation type identifier.</param>
    /// <param name="updateDto"></param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateType(Guid id, OperationTypeUpdateDto updateDto, CancellationToken ct)
    {
        await _operationTypeService.UpdateTypeAsync(id, updateDto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes an operation type
    /// </summary>
    /// <param name="id">Operation type identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteType(Guid id, CancellationToken ct)
    {
        await _operationTypeService.DeleteTypeAsync(id, ct);
        return NoContent();
    }
}
