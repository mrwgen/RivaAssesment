namespace RivaAssessment.Services;

/// <summary>
/// Service for managing user authentication and session timeout.
/// This service must:
/// - Track user activity timestamps
/// - Implement a configurable timeout period (e.g., 30 minutes)
/// - Validate session validity without impacting the 20ms latency requirement
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user and validates their session.
    /// Checks if the user's session is still valid based on activity timestamps and timeout period.
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task AuthenticateAsync(string userId);
}

