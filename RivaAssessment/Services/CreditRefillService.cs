using Microsoft.Extensions.Options;
using RivaAssessment.Models;
using RivaAssessment.Repositories;
using System.Threading;

namespace RivaAssessment.Services;

/// <summary>
/// Credit refill service implementation.
/// 
/// TODO: Implement this service to ensure all users have at least 5 credits every hour.
/// 
/// Requirements:
/// 1. Implement a background service or scheduled job that calls RefillCreditsForAllUsersAsync regularly
/// 2. Ensure users have at least 5 credits (refill to 5 if they have fewer)
/// 3. Update the cache in CreditService when credits are refilled
/// 4. Handle concurrent refill operations safely
/// 5. Consider implementing a maximum credit limit
/// 6. Ensure the refill operation is atomic and thread-safe
/// </summary>
public class CreditRefillService : ICreditRefillService
{
    private readonly CreditRefillOptions _creditRefillOptions;
    private readonly ILegacyBillingRepository _billingRepository;
    private readonly ICreditService _creditService;
    private readonly ILogger<CreditRefillService> _logger;

    public CreditRefillService(
        IOptions<CreditRefillOptions> creditRefillOptions,
        ILegacyBillingRepository billingRepository,
        ICreditService creditService,
        ILogger<CreditRefillService> logger)
    {
        _creditRefillOptions = creditRefillOptions.Value;
        _billingRepository = billingRepository;
        _creditService = creditService;
        _logger = logger;
    }

    public async Task RefillCreditsForAllUsersAsync(IEnumerable<string> users,CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(users, async (userId, crt) =>
         {
             try
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 var credits = await _billingRepository.GetCreditsAsync(userId);

                 //ensures max and min credits are enforced/idompotent
                 var newCredits = Math.Clamp(credits, _creditRefillOptions.MinimumCredits, _creditRefillOptions.MaximumCredits);

                 if (credits != newCredits)
                 {
                     _logger.LogInformation("Refilling credits for user {UserId} from {OldCredits} to {NewCredits}", userId, credits, newCredits);
                     await _billingRepository.SetCreditsAsync(userId, newCredits);
                     await _creditService.SetCredits(userId, newCredits);
                 }

             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error refilling credits for user {UserId}", userId);
             }
         });
    }
}