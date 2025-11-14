using FinanceTracker.Application.Interfaces.Services;

namespace FinanceTracker.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
    }

    /// <inheritdoc/>
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
