
using Microsoft.Extensions.Options;
using RivaAssessment.Models;
using System.Collections.Concurrent;

namespace RivaAssessment.Services;

/// <summary>
/// Authentication service implementation.
/// 
/// TODO: Implement this service to handle authentication timeout mechanism.
/// 
/// Requirements:
/// 1. Track user activity timestamps
/// 2. Implement a configurable timeout period (e.g., 30 minutes)
/// 3. When a user's session times out, require re-authentication (return false)
/// 4. Consider implementing a sliding expiration (reset timeout on activity)
/// 5. Ensure session validation doesn't impact the 20ms latency requirement
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly AuthenticationOptions _options;
    private readonly ConcurrentDictionary<string, DateTime> _userActivity = new();

    public AuthenticationService(ILogger<AuthenticationService> logger,IOptions<AuthenticationOptions>  authenticationOptions)
    {
        _logger = logger;
        _options= authenticationOptions.Value;
    }

    public Task AuthenticateAsync(string userId)
    {
        _logger.LogInformation("Authenticating user {UserId},{Time}", userId,DateTime.UtcNow);
        _userActivity.AddOrUpdate(userId, DateTime.UtcNow, (key, lastSeen) => {
            var now=DateTime.UtcNow;
            if (now - lastSeen > _options.SessionTimeout)
            {
                _logger.LogWarning("User {UserId} session has timed out. Last activity was at {LastSeen}", userId, lastSeen);
                throw new UnauthorizedAccessException("Session has timed out. Please re-authenticate.");
            }
            return now;
        });
        return Task.CompletedTask;
    }
}

