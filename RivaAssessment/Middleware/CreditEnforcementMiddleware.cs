using RivaAssessment.Services;

namespace RivaAssessment.Middleware;

/// <summary>
/// Middleware that enforces credit limits on every request.
/// 
/// Requirements:
/// - Intercept every HTTP request
/// - If user has credits: Deduct 1 and allow request to proceed
/// - If user has 0 credits: Return 402 Payment Required
/// - Must handle concurrency (race conditions)
/// - Must add less than 20ms latency
/// </summary>
public class CreditEnforcementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CreditEnforcementMiddleware> _logger;

    public CreditEnforcementMiddleware(
        RequestDelegate next,
        ILogger<CreditEnforcementMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    /// <summary>
    /// Processes an HTTP request by authenticating the user and enforcing credit requirements before allowing the
    /// request to proceed. 
    /// </summary>
    /// <remarks>This middleware expects the 'X-User-Id' header to be present in the request. If the user is
    /// not authenticated or does not have sufficient credits, the middleware will terminate the request with an
    /// appropriate HTTP status code. Otherwise, the request is passed to the next middleware in the pipeline.</remarks>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
    /// <param name="creditService">The service used to check and deduct user credits required to process the request.</param>
    /// <param name="authenticationService">The service used to authenticate the user based on the provided user ID.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context,
        ICreditService creditService,
        IAuthenticationService  authenticationService)
    {
        // TODO: Implement the middleware
        // - Extract user ID from X-User-Id header
        // - Call creditService.TryDeductCreditAsync() to deduct credit
        // - Return 402 if no credits available
        // - Allow request to proceed if credit deducted

        // I could add another Middleware to make the authentication part, but the requirement is to do it in this Middleware
        // In a larger systems I would separate the concerns, for now I want to keep it simple and in one place

        if (!context.Request.Headers.TryGetValue("X-User-Id", out var userIdValues))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing X-User-Id header");
            return;
        }
       var userId = userIdValues.ToString();
        try 
        {
            // Authenticate user
            await authenticationService.AuthenticateAsync(userId);
            // Try to deduct credit
            var creditDeducted = await creditService.TryDeductCreditAsync(userId);
            if (!creditDeducted)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                await context.Response.WriteAsync("Insufficient credits");
                return;
            }
            await _next(context);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Unauthorized access attempt for user {UserId}", userId);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Session expired");
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreditEnforcementMiddleware {userId}", userId);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error");
        }
      
    }
}
