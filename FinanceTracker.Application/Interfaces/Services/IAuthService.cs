using FinanceTracker.Application.DTOs.Auth;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
