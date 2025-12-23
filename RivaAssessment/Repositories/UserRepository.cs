
namespace RivaAssessment.Repositories
{
    /// <summary>
    /// Provides methods for retrieving user identifiers from the data source.  
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IEnumerable<string> _users = new List<string>
        {   "user1",
            "user2",
            "user3",
            "user4"
        };

        public Task<IEnumerable<string>> GetAllUserIds(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_users.AsEnumerable());
        }
    }
}
