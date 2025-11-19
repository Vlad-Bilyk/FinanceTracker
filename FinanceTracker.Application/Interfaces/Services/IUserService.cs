using FinanceTracker.Application.DTOs.User;

namespace FinanceTracker.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> GetCurrentUserAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task UpdateUserAsync(Guid id, UserUpdateDto updateDto, CancellationToken ct = default);
    Task DeleteUserAsync(Guid id, CancellationToken ct = default);
    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);
}
