
using Microsoft.Extensions.Options;
using RivaAssessment.Models;
using RivaAssessment.Repositories;
using RivaAssessment.Services;

namespace RivaAssessment.BackgroundJobs
{
    public class CreditRefillBackgroundService : BackgroundService
    {
        private readonly ILogger<CreditRefillBackgroundService> _logger;
        private readonly ICreditRefillService _creditRefillService;
        private readonly TimeSpan _interval;
        private readonly IUserRepository _userRepository;

        public CreditRefillBackgroundService(ILogger<CreditRefillBackgroundService> logger,
            ICreditRefillService creditRefillService,
            IUserRepository userRepository,
            IOptions<CreditRefillOptions> options)
        {
            _creditRefillService = creditRefillService;
            _interval = options.Value.RefillIntervalMinutes;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Executes the background service operation that periodically refills credits for all users until the service
        /// is stopped. 
        /// </summary>
        /// <remarks>The method runs continuously until the provided cancellation token is signaled. It
        /// logs the start and completion of each credit refill cycle, and handles any exceptions that occur during
        /// execution. This method is typically not called directly; it is invoked by the hosting infrastructure when
        /// the service starts.</remarks>
        /// <param name="stoppingToken">A cancellation token that can be used to request the termination of the background operation.</param>
        /// <returns>A task that represents the asynchronous execution of the background service.</returns>
        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Credit refill background service is starting a new cycle at: {time}", DateTimeOffset.Now);
                try
                {
                    var userIds = await _userRepository.GetAllUserIds(stoppingToken);

                    await _creditRefillService.RefillCreditsForAllUsersAsync(userIds, stoppingToken);

                    _logger.LogInformation("Credit refill background service completed a cycle at: {time}", DateTimeOffset.Now);
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the credit refill process at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
