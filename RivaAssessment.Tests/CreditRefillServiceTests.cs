using Moq;
using RivaAssessment.Repositories;
using Microsoft.Extensions.Logging;
using RivaAssessment.Services;
using RivaAssessment.Models;
using Xunit;
using Microsoft.Extensions.Options;

namespace RivaAssessment.Tests
{
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
