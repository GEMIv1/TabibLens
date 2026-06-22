using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<User?> GetWithRefreshTokensAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetWithPrescriptionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetWithChatSessionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailWithRefreshTokensAsync(string email, CancellationToken cancellationToken = default);
    }
}
