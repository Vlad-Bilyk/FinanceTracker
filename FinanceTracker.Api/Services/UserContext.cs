using FinanceTracker.Application.Interfaces.Common;
using System.Security.Claims;

namespace FinanceTracker.Api.Services;

/// <summary>
/// Provides access to the current user's context based on the HTTP request.
/// </summary>
public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContext"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContextAccessor"/> is null.</exception>
    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    public Guid? UserId
    {
        get
        {
            var id = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(id, out var userId) ? userId : null;
        }
    }

    /// <inheritdoc/>
    public Guid GetRequiredUserId()
    {
        return UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
    }
}

