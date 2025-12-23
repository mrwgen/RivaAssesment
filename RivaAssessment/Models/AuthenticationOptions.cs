namespace RivaAssessment.Models
{
    public sealed class AuthenticationOptions
    {
        public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(30);
    }
}
