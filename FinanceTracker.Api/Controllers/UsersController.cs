using FinanceTracker.Application.DTOs.User;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Manages user accounts.
/// </summary>
[Route("api/users")]
[Authorize]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService">Service for working with users.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UsersController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Returns the user if found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken ct)
    {
        var user = await _userService.GetUserByIdAsync(id, ct);
        return Ok(user);
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Returns the list of users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    /// <summary>
    /// Updates user data.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="updateDto">Data for update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Returns 204 No Content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateUser(Guid id, UserUpdateDto updateDto, CancellationToken ct)
    {
        await _userService.UpdateUserAsync(id, updateDto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Returns 204 No Content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await _userService.DeleteUserAsync(id, ct);
        return NoContent();
    }

    /// <summary>
    ///  Changes the password for the currently authenticated user.
    /// </summary>
    /// <param name="request">Password change data (current and new password).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Returns 204 No Content if the password was changed successfully.</returns>
    [HttpPut("me/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        await _userService.ChangePasswordAsync(request, ct);
        return NoContent();
    }
}
