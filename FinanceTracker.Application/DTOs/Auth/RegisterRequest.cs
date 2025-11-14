namespace FinanceTracker.Application.DTOs.Auth;

public record RegisterRequest(
    string UserName,
    string Password
);