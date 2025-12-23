
using Microsoft.Extensions.Options;
using RivaAssessment.Models;

namespace RivaAssessment.Infrastructure
{
    /// <summary>
    /// Provides a retry policy for executing asynchronous operations with configurable retry attempts and delay between
    /// retries.    
    /// </summary>
    /// <remarks>The retry behavior, including the maximum number of attempts and the delay between retries,
    /// is determined by the provided configuration options. This class is typically used to add resilience to
    /// operations that may fail transiently, such as network or database calls. Logging is performed for each failed
    /// attempt and when the maximum number of retries is reached.</remarks>
    public class RetryPolicy : IRetryPolicy
    {
        private readonly CreditPersistenceOptions _options;
        private readonly ILogger<RetryPolicy> _logger;
        public RetryPolicy(IOptions<CreditPersistenceOptions> creditPersistenceOptions,ILogger<RetryPolicy> logger)
        {
            _options = creditPersistenceOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Executes the specified asynchronous operation with retry logic, retrying on failure up to the configured
        /// maximum number of attempts. 
        /// </summary>
        /// <remarks>If the operation fails and cancellation has not been requested, it is retried after a
        /// delay, up to the maximum number of attempts as configured. If the maximum number of attempts is reached
        /// without success, the last exception is rethrown. If cancellation is requested, the method stops retrying and
        /// propagates the cancellation.</remarks>
        /// <param name="operation">A delegate representing the asynchronous operation to execute. The operation is retried if it throws an
        /// exception and the cancellation has not been requested.</param>
        /// <param name="operationName">A name used to identify the operation in log messages. This value is included in error and retry logs.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the execution and any pending retries. The operation is not
        /// retried if cancellation is requested.</param>
        /// <returns>A task that represents the asynchronous execution of the operation, including any retries. The task
        /// completes when the operation succeeds, is canceled, or the maximum number of retry attempts is reached.</returns>
        public async Task ExecuteAsync(Func<Task> operation, string operationName, CancellationToken cancellationToken = default)
        {
            for (int attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Operation {OperationName} failed on attempt {Attempt}. Retrying after {Delay}ms.", operationName, attempt, _options.RetryDelayMilliseconds);
                    await Task.Delay(_options.RetryDelayMilliseconds, cancellationToken);

                    if(attempt == _options.MaxRetryAttempts)
                    {
                        _logger.LogCritical(ex, "Operation {OperationName} failed after {MaxAttempts} attempts.", operationName, _options.MaxRetryAttempts);
                        throw;
                    }
                    await Task.Delay(_options.RetryDelayMilliseconds, cancellationToken);
                }
            }
        }
    }
}
