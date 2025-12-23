namespace RivaAssessment.Services;

/// <summary>
/// Service for managing credit refills to ensure all users have at least 5 credits every hour.
/// This service should be implemented as a background service or scheduled job.
/// </summary>
public interface ICreditRefillService
{
/// <summary>
/// Refills the available credits for each user in the specified collection asynchronously.
/// </summary>
/// <param name="users">A collection of user identifiers for which to refill credits. Each identifier must be a non-null, non-empty string.</param>
/// <returns>A task that represents the asynchronous refill operation.</returns>
    Task RefillCreditsForAllUsersAsync(IEnumerable<string> users,CancellationToken cancellationToken);
}

