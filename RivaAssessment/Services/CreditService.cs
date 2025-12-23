using RivaAssessment.Infrastructure;
using RivaAssessment.Models;
using RivaAssessment.Repositories;
using System.Collections.Concurrent;

namespace RivaAssessment.Services;

/// <summary>
/// Credit service implementation.
/// 
/// TODO: Implement this service with:
/// 1. Caching strategy (e.g., Token Bucket or Cache-Aside pattern)
/// 2. Concurrency control (locking, atomic operations, or optimistic concurrency)
/// 3. Performance optimization to meet 20ms latency requirement
/// </summary>
public class CreditService : ICreditService
{
    private readonly ILegacyBillingRepository _billingRepository;
    private readonly IRetryPolicy _retryPolicy;
    private readonly ILogger<CreditService> _logger;
    private readonly ConcurrentDictionary<string, CreditCacheEntry> _cache = new();
    public CreditService(
        ILegacyBillingRepository billingRepository,
        IRetryPolicy retryPolicy,
        ILogger<CreditService> logger
       )
    {
        _billingRepository = billingRepository;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }
    public async Task<bool> TryDeductCreditAsync(string userId)
    {
        // TODO: Implement credit deduction with:
        // - Caching to avoid 100ms database calls
        // - Atomic operations to prevent race conditions
        // - Performance optimization (< 20ms latency)

        var entry = _cache.GetOrAdd(userId, _ => new CreditCacheEntry());
        _logger.LogInformation("Attempting to acquire credit lock for user {UserId}", userId);
        await entry.Lock.WaitAsync();
        try
        {
            if (!entry.IsInitialized)
            {
                _logger.LogInformation("Cache miss for user {UserId}, fetching from database", userId);
                entry.Credits = await _billingRepository.GetCreditsAsync(userId);
                entry.IsInitialized = true;

            }
            if (entry.Credits == 0)
            {
                _logger.LogInformation("Insufficient credits for user {UserId}", userId);
                return false;
            }
            entry.Credits--;
            _logger.LogInformation("Credit deducted for user {UserId}, remaining credits: {Credits}", userId, entry.Credits);

            this.PersistDeduction(userId);
            return true;
        }
        finally
        {
            entry.Lock.Release();
        }

    }

    public async Task SetCredits(string userId, int credits)
    {
        var entry = _cache.GetOrAdd(userId, _ => new CreditCacheEntry());
        await entry.Lock.WaitAsync();
        try
        {
            entry.Credits = credits;

            _logger.LogInformation("Setting credits for user {UserId} to {Credits}", userId, credits);
            await _billingRepository.SetCreditsAsync(userId, credits);
        }
        finally
        {
            entry.Lock.Release();
        }
    }

    private void PersistDeduction(string userId)
    {
        _ = _retryPolicy.ExecuteAsync(() =>
                 _billingRepository.DeductCreditsAsync(userId, 1),
            operationName: $"DeductCredit-{userId}"
        );

    }

    public async Task<int> GetCredits(string userId)
    {
        var entry = _cache.GetOrAdd(userId, _ => new CreditCacheEntry());
        await entry.Lock.WaitAsync();

        try
        {
            if (!entry.IsInitialized)
            {
                _logger.LogInformation("Cache miss for user {UserId}, fetching from database", userId);
                entry.Credits = await _billingRepository.GetCreditsAsync(userId);
                entry.IsInitialized = true;
            }
            return entry.Credits;
        }
        finally
        {
            entry.Lock.Release();
        }
    }
}


