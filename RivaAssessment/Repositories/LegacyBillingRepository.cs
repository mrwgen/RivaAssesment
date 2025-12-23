namespace RivaAssessment.Repositories;

/// <summary>
/// Legacy billing repository that simulates a slow database.
/// This has a simulated latency of ~100ms per operation.
/// DO NOT call this directly on every request - you must implement caching!
/// 
/// IMPORTANT: The code in this class and ILegacyBillingRepository should NOT be adjusted.
/// These classes are provided as-is and should be used as specified.
/// </summary>
public class LegacyBillingRepository : ILegacyBillingRepository
{
    // In-memory storage for simulation purposes
    // In a real scenario, this would be a database
    private readonly Dictionary<string, int> _creditStore = new()
    {
        { "user1", 10 },
        { "user2", 5 },
        { "user3", 0 },
        { "user4", 100 }
    };

    private readonly object _lock = new();

    public async Task<int> GetCreditsAsync(string userId)
    {
        // Simulate database latency
        await Task.Delay(100);

        lock (_lock)
        {
            return _creditStore.TryGetValue(userId, out var credits) ? credits : 0;
        }
    }

    public async Task<bool> DeductCreditsAsync(string userId, int amount)
    {
        // Simulate database latency
        await Task.Delay(100);

        lock (_lock)
        {
            if (!_creditStore.TryGetValue(userId, out var currentCredits))
            {
                _creditStore[userId] = 0;
                return false;
            }

            if (currentCredits < amount)
            {
                return false;
            }

            _creditStore[userId] = currentCredits - amount;
            return true;
        }
    }

    public async Task SetCreditsAsync(string userId, int amount)
    {
        // Simulate database latency
        await Task.Delay(100);

        lock (_lock)
        {
            _creditStore[userId] = amount;
        }
    }
}


