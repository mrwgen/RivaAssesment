using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RivaAssessment.Models;
using RivaAssessment.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RivaAssessment.Tests
{
    /// <summary>
    /// Tests for the Authentication Service
    /// </summary>
    public class AuthenticationServiceTests
    {
        private readonly Mock<ILogger<AuthenticationService>> _logger;
        private readonly Mock<IOptions<AuthenticationOptions>> _options;
        public AuthenticationServiceTests()
        {
            _logger = new Mock<ILogger<AuthenticationService>>();
            _options= new Mock<IOptions<AuthenticationOptions>>();
        }

        [Fact]
        public async Task AuthenticateAsync_ThrowsUnauthorizedAccessException_WhenSessionTimedOut()
        {
            // Arrange
            var sessionTimeout = TimeSpan.FromMicroseconds(10);
            _options.Setup(x => x.Value).Returns(new AuthenticationOptions() { SessionTimeout = sessionTimeout });
            
           
            var service = new AuthenticationService(_logger.Object, _options.Object);
            var userId = "user1";
            // Act
            await service.AuthenticateAsync(userId);
            // Simulate time passing beyond the session timeout
            await Task.Delay(sessionTimeout+ TimeSpan.FromSeconds(1));
            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AuthenticateAsync(userId));
        }
        [Fact]
        public async Task AuthenticateAsync_ExtendsSession_OnActivity()
        {
            //Arrange
            var sessionTimeout = TimeSpan.FromMilliseconds(50);
            _options.Setup(x => x.Value).Returns(new AuthenticationOptions() { SessionTimeout = sessionTimeout });
            var service = new AuthenticationService(_logger.Object, _options.Object);
            var userId = "user2";
            // Act
            await service.AuthenticateAsync(userId);
            // Simulate activity before session timeout
            await Task.Delay(sessionTimeout - TimeSpan.FromMilliseconds(20));
            await service.AuthenticateAsync(userId);
            // Simulate more time passing, but within extended session timeout
            await Task.Delay(sessionTimeout - TimeSpan.FromMilliseconds(20));
            // Assert
            await service.AuthenticateAsync(userId); // Should not throw
        }

    }
}