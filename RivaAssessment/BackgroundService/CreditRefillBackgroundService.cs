
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
