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
  

    public CreditServiceTests()
    {
        _repositoryMock = new Mock<ILegacyBillingRepository>();
        _loggerMock = new Mock<ILogger<CreditService>>();

        _retryPolicyMock
           .Setup(r => r.ExecuteAsync(It.IsAny<Func<Task>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .Returns<Func<Task>, string, CancellationToken>((op, name, ct) => op());

    }

    // TODO: Implement tests for CreditService
    [Fact]
    public async Task TryDeductCredit_ReturnsFalse_WhenUserHasNoCredits()
    {
        _repositoryMock.Setup(r => r.GetCreditsAsync("user3")).ReturnsAsync(0);
   
        var service=new CreditService(_repositoryMock.Object,_retryPolicyMock.Object, _loggerMock.Object);

        var result = await service.TryDeductCreditAsync("user3");
        Assert.False(result);
    }
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
