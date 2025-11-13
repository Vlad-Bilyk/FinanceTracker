using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

/// <summary>
/// Provides endpoints for user authentication and registration.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">Registration data provided by the client.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        await _authService.RegisterAsync(request, ct);
        return Ok();
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <param name="request">Login credentials provided by the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Returns HTTP 200 OK with a JWT token in the response body
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var token = await _authService.LoginAsync(request, ct);
        return Ok(token);
    }
}
