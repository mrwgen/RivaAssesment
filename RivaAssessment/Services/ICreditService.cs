namespace RivaAssessment.Services;

/// <summary>
/// Service for managing user credits with caching and concurrency control.
/// This service must:
/// - Use caching to avoid calling LegacyBillingRepository on every request (20ms latency requirement)
/// - Handle race conditions (atomic operations)
/// - Ensure only one request succeeds when user has 1 credit and 10 concurrent requests arrive
/// </summary>
public interface ICreditService
{
    /// <summary>
    /// Attempts to deduct 1 credit from the user's account.
    /// This operation must be atomic and handle concurrency.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>True if credit was successfully deducted, false if insufficient credits</returns>
    Task<bool> TryDeductCreditAsync(string userId);

    /// <summary>
    /// User credits in cache will be set here. 
    /// This is here to avoid concurrency issues. Credit refill will be calling it to refil user cache as well.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="credits"></param>
    /// <returns></returns>
    Task SetCredits(string userId, int credits);

    /// <summary>
    /// Asynchronously retrieves the current credit balance for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose credit balance is to be retrieved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of credits available to
    /// the user.</returns>
    Task<int> GetCredits(string userId);
}


