namespace FinanceTracker.Application.DTOs.Auth;

public record LoginRequest(
    string UserName,
    string Password
);
