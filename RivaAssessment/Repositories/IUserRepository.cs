namespace RivaAssessment.Repositories
{
    /// <summary>
    /// Defines a contract for retrieving user identifiers from a data source.  
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Asynchronously retrieves the identifiers of all users.  
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of strings
        /// representing the user identifiers. The collection is empty if no users are found.</returns>
        Task<IEnumerable<string>> GetAllUserIds(CancellationToken cancellationToken);
    }
}
