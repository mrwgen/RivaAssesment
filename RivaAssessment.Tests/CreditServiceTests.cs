using Microsoft.Extensions.Logging;
using Moq;
using RivaAssessment.Infrastructure;
using RivaAssessment.Repositories;
using RivaAssessment.Services;
using Xunit;

namespace RivaAssessment.Tests;

/// <summary>
/// Tests for the Credit Service
/// </summary>
public class CreditServiceTests
{
    private readonly Mock<ILegacyBillingRepository> _repositoryMock;
    private readonly Mock<ILogger<CreditService>> _loggerMock;
    private readonly Mock<IRetryPolicy> _retryPolicyMock = new();
  
    /// <summary>
    /// Initializes a new instance of the CreditServiceTests class with mocked dependencies for unit testing.   
    /// </summary>
    /// <remarks>This constructor sets up mock implementations for the ILegacyBillingRepository and
    /// ILogger<CreditService> interfaces, as well as a retry policy mock. These mocks are used to isolate the
    /// CreditService during tests and to control the behavior of its dependencies.</remarks>
    public CreditServiceTests()
    {
        _repositoryMock = new Mock<ILegacyBillingRepository>();
        _loggerMock = new Mock<ILogger<CreditService>>();

        _retryPolicyMock
           .Setup(r => r.ExecuteAsync(It.IsAny<Func<Task>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .Returns<Func<Task>, string, CancellationToken>((op, name, ct) => op());

    }

   /// <summary>
   /// Verifies that the TryDeductCreditAsync method returns false when the user has no available credits.      
   /// </summary>
   /// <remarks>This test ensures that attempting to deduct a credit from a user with zero credits does not
   /// succeed. It sets up the repository mock to return zero credits for the specified user and asserts that the
   /// service method returns false.</remarks>
   /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task TryDeductCredit_ReturnsFalse_WhenUserHasNoCredits()
    {
        _repositoryMock.Setup(r => r.GetCreditsAsync("user3")).ReturnsAsync(0);
   
        var service=new CreditService(_repositoryMock.Object,_retryPolicyMock.Object, _loggerMock.Object);

        var result = await service.TryDeductCreditAsync("user3");
        Assert.False(result);
    }
    /// <summary>
    /// Verifies that when multiple concurrent attempts are made to deduct a single available credit, only one attempt
    /// succeeds and the others fail.   
    /// </summary>
    /// <remarks>This test ensures that the credit deduction logic is thread-safe and prevents multiple
    /// deductions when only one credit is available. It simulates concurrent requests and asserts that only one
    /// operation can successfully deduct the credit, while all others are unsuccessful.</remarks>
    /// <returns></returns>
    [Fact]
    public async Task ConCurrentDeduction_WithOneCredit_OnlyOneSucceeds()
    {
        _repositoryMock.Setup(r => r.GetCreditsAsync("user3")).ReturnsAsync(1);
        var service=new CreditService(_repositoryMock.Object,_retryPolicyMock.Object, _loggerMock.Object);  
        var tasks=Enumerable.Range(0,10).
            Select(_=> Task.Run(() => service.TryDeductCreditAsync("user3"))).ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, results.Count(r => r)); 
        Assert.Equal(9, results.Count(r => !r));

    }
    /// <summary>
    /// Verifies that the TryDeductCreditAsync method uses a cached value for subsequent calls after the first call for
    /// the same user.      
    /// </summary>
    /// <remarks>This test ensures that the credit retrieval operation from the repository is performed only
    /// once for repeated calls with the same user identifier, confirming that caching is implemented correctly in the
    /// CreditService.</remarks>
    /// <returns></returns>

    [Fact]
    public async Task TryDeductCredit_UsesCache_AfterFirstCall()
    {    
        _repositoryMock.Setup(r => r.GetCreditsAsync("user4")).ReturnsAsync(3);

        var service=new CreditService(_repositoryMock.Object,_retryPolicyMock.Object, _loggerMock.Object);  
        var result1 = await service.TryDeductCreditAsync("user4");
        var result2 = await service.TryDeductCreditAsync("user4");
        var result3 = await service.TryDeductCreditAsync("user4");
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        _repositoryMock.Verify(r => r.GetCreditsAsync("user4"), Times.Once);
    }
    /// <summary>
    /// Verifies that calling TryDeductCreditAsync for a user with cached credit information completes quickly. 
    /// </summary>
    /// <remarks>This test ensures that repeated calls to TryDeductCreditAsync for different users leverage
    /// caching or other optimizations to maintain fast execution times. The test asserts that the second call completes
    /// within a short time frame, indicating efficient retrieval of credit data.</remarks>
    /// <returns></returns>
    [Fact]
    public async Task TryDeductCredit_CachedCall_IsFast()
    {
        //Arrange
        _repositoryMock.Setup(r => r.GetCreditsAsync("user4"))
            .ReturnsAsync(20);

        var service=new CreditService(_repositoryMock.Object,_retryPolicyMock.Object, _loggerMock.Object);  
        //act
        var result1 = await service.TryDeductCreditAsync("user4");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result2 = await service.TryDeductCreditAsync("user5");
         stopwatch.Stop();

        //assert
        Assert.True(stopwatch.ElapsedMilliseconds<20);
    
  
    }
}
