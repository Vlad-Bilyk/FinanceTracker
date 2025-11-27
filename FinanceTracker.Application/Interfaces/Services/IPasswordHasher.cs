namespace FinanceTracker.Application.Interfaces.Services;

/// <summary>
/// Defines methods for hashing passwords and verifying password hashes.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Provides password hashing and verification functionality.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password string.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a plain text password matches a hashed password.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hash">The hashed password to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}
