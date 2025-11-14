namespace FinanceTracker.Application.Interfaces.Services;

/// <summary>
/// Provides JWT token generation and validation services for authentication.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="username">The username of the user.</param>
    /// <returns>A JWT token string that can be used for authentication.</returns>
    string GenerateToken(Guid userId, string username);

    /// <summary>
    /// Validates a JWT token and extracts the user ID from it.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>The user ID if the token is valid; otherwise, null.</returns>
    Guid? ValidateToken(string token);
}
