using Moq;
using RivaAssessment.Repositories;
using Microsoft.Extensions.Logging;
using RivaAssessment.Services;
using RivaAssessment.Models;
using Xunit;
using Microsoft.Extensions.Options;

namespace RivaAssessment.Tests
{
    /// <summary>
    /// Contains unit tests for the CreditRefillService, verifying correct credit refill behavior under various user
    /// credit scenarios.
    /// </summary>
    /// <remarks>These tests ensure that the CreditRefillService correctly refills user credits when below the
    /// minimum, clamps credits to the maximum when above, does nothing when credits are within the allowed range, and
    /// continues processing other users if an error occurs for a single user. The tests use mock dependencies to
    /// isolate service logic and validate interactions with external components.</remarks>
    public class CreditRefillServiceTests
    {
        private readonly Mock<ILegacyBillingRepository> _bilingRepoMock;
        private readonly Mock<ICreditService> _creditServiceMock;
        private readonly Mock<ILogger<CreditRefillService>> _loggerMock;
        private readonly CreditRefillService _service;
        private readonly Mock<IOptions<CreditRefillOptions>> _options;

        public CreditRefillServiceTests()
        {
            _bilingRepoMock = new Mock<ILegacyBillingRepository>();
            _creditServiceMock = new Mock<Services.ICreditService>();
            _loggerMock = new Mock<ILogger<Services.CreditRefillService>>();
            _options=new Mock<IOptions<CreditRefillOptions>>();

            _options.Setup(x=>x.Value).Returns( new CreditRefillOptions
            {
                MinimumCredits = 5,
                MaximumCredits = 20
            });

            _service = new CreditRefillService(
                _options.Object,
                _bilingRepoMock.Object,
                _creditServiceMock.Object,
                _loggerMock.Object
                );
        }

        /// <summary>
        /// Verifies that the RefillCreditsForAllUsersAsync method refills a user's credits when their balance is below
        /// the minimum threshold.
        /// </summary>
        /// <remarks>This test ensures that when a user's credit balance is below the required minimum,
        /// the service correctly updates the user's credits to the expected value. It uses mocked dependencies to
        /// validate that the appropriate methods are called with the correct parameters.</remarks>
        /// <returns></returns>
        [Fact]
        public async Task RefillCredits_RefillsUser_WhenBelowMinimum()
        {   
            //Arrange
            var users = new [] { "user1"};
            _bilingRepoMock.Setup(r => r.GetCreditsAsync("user1")).ReturnsAsync(2);

            //Act
            await _service.RefillCreditsForAllUsersAsync(users, CancellationToken.None);

            //Assert
            _bilingRepoMock.Verify(r => r.SetCreditsAsync("user1", 5), Times.Once);
            _creditServiceMock.Verify(c => c.SetCredits("user1", 5), Times.Once);
      
        }
        /// <summary>
        /// Verifies that no credits are refilled when a user's existing credits are already within the allowed range.  
        /// </summary>
        /// <remarks>This test ensures that the credit refill service does not perform unnecessary updates
        /// when the user's credit balance does not require adjustment.</remarks>
        /// <returns>A task that represents the asynchronous test operation.</returns>

        [Fact]
        public async Task RefillCredits_DoesNothing_WhenCreditsAlreadyInRange()
        {
             //Arrange
             var users = new[] { "user2" };
              _bilingRepoMock.Setup(r => r.GetCreditsAsync("user2")).ReturnsAsync(10);
            //Act

            await _service.RefillCreditsForAllUsersAsync(users, CancellationToken.None);

            //Assert
            _bilingRepoMock.Verify(r => r.SetCreditsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            _creditServiceMock.Verify(c => c.SetCredits(It.IsAny<string>(), It.IsAny<int>()), Times.Never);

        }
        /// <summary>
        /// Verifies that the RefillCreditsForAllUsersAsync method clamps a user's credits to the maximum allowed value
        /// when the current credits exceed the maximum.
        /// </summary>
        /// <remarks>This test ensures that when a user's credits are above the defined maximum, the
        /// service sets the credits to the maximum value rather than leaving them unchanged or exceeding the
        /// limit.</remarks>
        /// <returns></returns>

        [Fact]
        public async Task RefillCredits_ClampsCredits_WhenAboveMaximum()
        { 
            //Arrange
            var users = new[] { "user3" };
            _bilingRepoMock.Setup(r => r.GetCreditsAsync("user3")).ReturnsAsync(25);
            //Act
            await _service.RefillCreditsForAllUsersAsync(users, CancellationToken.None);
            //Assert
            _bilingRepoMock.Verify(r => r.SetCreditsAsync("user3", 20), Times.Once);
            _creditServiceMock.Verify(c => c.SetCredits("user3", 20), Times.Once);

        }
        /// <summary>
        /// Verifies that the RefillCreditsForAllUsersAsync method continues processing remaining users when an
        /// exception occurs for one user.  
        /// </summary>
        /// <remarks>This test ensures that a failure to refill credits for a single user does not prevent
        /// the method from attempting to refill credits for other users in the collection.</remarks>
        /// <returns></returns>
        [Fact]
        public async Task RefillCredits_Continues_WhenOneUserFails()
        {
            //Arrange
            var users = new[] { "user1", "user2" };
            _bilingRepoMock.Setup(r => r.GetCreditsAsync("user1")).ReturnsAsync(2);
            _bilingRepoMock.Setup(r => r.GetCreditsAsync("user2")).ThrowsAsync(new Exception("Database error"));

            //Act   
            await _service.RefillCreditsForAllUsersAsync(users, CancellationToken.None);

            //Assert
            _bilingRepoMock.Verify(r => r.SetCreditsAsync("user1", 5), Times.Once);
                

        }

    }
    }
