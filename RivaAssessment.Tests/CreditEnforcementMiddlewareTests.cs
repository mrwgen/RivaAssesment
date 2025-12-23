using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using RivaAssessment.Middleware;
using RivaAssessment.Services;
using Xunit;

namespace RivaAssessment.Tests;

/// <summary>
/// Tests for the Credit Enforcement Middleware
/// </summary>
public class CreditEnforcementMiddlewareTests
{
    private readonly Mock<ICreditService> _creditServiceMock;
    private readonly Mock<ILogger<CreditEnforcementMiddleware>> _loggerMock;
    public readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly RequestDelegate _next;

    public CreditEnforcementMiddlewareTests()
    {
        _creditServiceMock = new Mock<ICreditService>();
        _loggerMock = new Mock<ILogger<CreditEnforcementMiddleware>>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _next = (HttpContext context) => Task.CompletedTask;
    }
    /// <summary>
    /// Creates a new instance of <see cref="DefaultHttpContext"/> with an optional user identifier set in the request
    /// headers.
    /// </summary>
    /// <param name="userId">The user identifier to assign to the 'X-User-Id' request header. If <see langword="null"/>, the header will be
    /// set to <see langword="null"/>.</param>
    /// <returns>A <see cref="DefaultHttpContext"/> instance with the response body initialized and the 'X-User-Id' header set as
    /// specified.</returns>
    private DefaultHttpContext CreateHttpContext(string? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        if (!string.IsNullOrEmpty(userId))
        {
            context.Request.Headers["X-User-Id"] = userId;
        }
        return context;
    }

    /// <summary>
    /// Asynchronously reads the entire response body from the specified HTTP context as a string.
    /// </summary>
    /// <remarks>The response body stream position is reset to the beginning before reading. Ensure that the
    /// response body stream supports seeking and reading before calling this method.</remarks>
    /// <param name="context">The HTTP context containing the response whose body is to be read. Must not be null.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the full response body as a
    /// string.</returns>
    private async Task<string> GetResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
    [Fact]
    public async Task InvokeAsync_Returns402_WhenUserHasNoCredits()
    {
        // Arrange
        var userId = "user1";
        var middleware = new CreditEnforcementMiddleware(
           _next,
           _loggerMock.Object);
        var context = CreateHttpContext(userId);
        _creditServiceMock
           .Setup(c => c.TryDeductCreditAsync(userId))
           .ReturnsAsync(false);
        _authenticationServiceMock
         .Setup(a => a.AuthenticateAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context, _creditServiceMock.Object, _authenticationServiceMock.Object);

        // Assert
        Assert.Equal(402, context.Response.StatusCode);
        var responseBody = await GetResponseBodyAsync(context);
        Assert.Contains("Insufficient credits", responseBody);
    }
    [Fact]
    public async Task InvokeAsync_AllowsRequest_WhenUserHasCredits()
    {
        // Arrange
        var userId = "user2";
        var middleware = new CreditEnforcementMiddleware(
           _next,
           _loggerMock.Object);
        var context = CreateHttpContext(userId);
        _creditServiceMock
           .Setup(c => c.TryDeductCreditAsync(userId))
           .ReturnsAsync(true);
        _authenticationServiceMock
         .Setup(a => a.AuthenticateAsync(userId)).Returns(Task.CompletedTask);
        // Act
        await middleware.InvokeAsync(context, _creditServiceMock.Object, _authenticationServiceMock.Object);
        // Assert
        Assert.Equal(200, context.Response.StatusCode);
    }
    [Fact]
    public async Task InvokeAsync_Returns401_WhenUserIdHeaderMissing()
    {
        // Arrange
        var middleware = new CreditEnforcementMiddleware(
           _next,
           _loggerMock.Object);
        var context = CreateHttpContext();
        // Act
        await middleware.InvokeAsync(context, _creditServiceMock.Object, _authenticationServiceMock.Object);
        // Assert
        Assert.Equal(401, context.Response.StatusCode);
        var responseBody = await GetResponseBodyAsync(context);
        Assert.Contains("Missing X-User-Id header", responseBody);
    }
    [Fact]
    public async Task UnexpectedError_Returns500()
    {
        // Arrange
        var userId = "user3";
        var middleware = new CreditEnforcementMiddleware(
           _next,
           _loggerMock.Object);
        var context = CreateHttpContext(userId);
        _authenticationServiceMock
         .Setup(a => a.AuthenticateAsync(userId)).ThrowsAsync(new Exception("Unexpected error"));
        // Act
        await middleware.InvokeAsync(context, _creditServiceMock.Object, _authenticationServiceMock.Object);
        // Assert
        Assert.Equal(500, context.Response.StatusCode);
        var responseBody = await GetResponseBodyAsync(context);
        Assert.Contains("Internal server error", responseBody);
    }
    [Fact]
    public async Task InvokeAsync_Returns401_MissingHeader()
    {
        // Arrange
        var middleware = new CreditEnforcementMiddleware(
           _next,
           _loggerMock.Object);
        var context = CreateHttpContext();
        // Act
        await middleware.InvokeAsync(context, _creditServiceMock.Object, _authenticationServiceMock.Object);
        // Assert
        Assert.Equal(401, context.Response.StatusCode);
        var responseBody = await GetResponseBodyAsync(context);
        Assert.Contains("Missing X-User-Id header", responseBody);
    }
}
