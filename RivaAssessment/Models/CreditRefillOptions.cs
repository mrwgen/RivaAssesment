namespace RivaAssessment.Models
{
    /// <summary>
    /// Represents configuration options for credit refill operations, including credit limits and refill intervals.    
    /// </summary>
    public sealed class CreditRefillOptions
    {
        public int MinimumCredits { get; set; } = 5;
        public int MaximumCredits { get; set; } = 20;
        public TimeSpan RefillIntervalMinutes { get; set; }=TimeSpan.FromMinutes(60);
    }
}
