namespace RivaAssessment.Models
{
    /// <summary>
    /// Represents a cache entry that holds the available credits and an associated lock for thread-safe operations.    
    /// </summary>
    /// <remarks>This class is typically used to manage concurrent access to a credit value, ensuring that
    /// updates to the credits are synchronized using the provided semaphore. The caller is responsible for managing the
    /// lifecycle of the semaphore and ensuring proper usage to avoid deadlocks.</remarks>
    public sealed class CreditCacheEntry
    {
        public int Credits { get; set; }

        public bool IsInitialized;
        public SemaphoreSlim Lock { get; set; }=new (1, 1);
    }
}
