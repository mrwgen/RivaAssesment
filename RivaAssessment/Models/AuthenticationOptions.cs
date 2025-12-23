namespace RivaAssessment.Models
{
    /// <summary>
    /// Represents configuration options for authentication behavior, such as session timeout settings. 
    /// </summary>
    public sealed class AuthenticationOptions
    {
        public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(30);
    }
}
