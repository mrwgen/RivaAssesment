namespace RivaAssessment.Infrastructure
{
    public interface IRetryPolicy
    {
        Task ExecuteAsync(Func<Task> operation,
                            string operationName,
                            CancellationToken cancellationToken = default);
    }
}
