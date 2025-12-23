namespace RivaAssessment.Models
{
    /// <summary>
    /// Represents configuration options for retrying credit persistence operations.    
    /// </summary>
    /// <remarks>Use this class to specify how many times a failed credit persistence operation should be
    /// retried and the delay between retries. These options can help control resilience and responsiveness when
    /// persisting credit data.</remarks>
    public sealed class  CreditPersistenceOptions
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMilliseconds { get; set; } = 500;
    }
}
