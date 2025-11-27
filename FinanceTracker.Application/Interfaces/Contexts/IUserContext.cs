namespace FinanceTracker.Application.Interfaces.Common;

/// <summary>
/// Exposes information about the current authenticated user for the active request.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the identifier of the current authenticated user from the JWT <c>sub</c> claim.
    /// </summary>
    /// <returns>
    /// A <see cref="Guid"/> representing the user identifier,
    /// or <c>null</c> if the user is not authenticated or the claim is missing/invalid.
    /// </returns>
    Guid? UserId { get; }

    /// <summary>
    /// Retrieves the unique identifier of the current user.
    /// </summary>
    /// <returns>A <see cref="Guid"/> representing the unique identifier of the current user.</returns>
    Guid GetRequiredUserId();
}
