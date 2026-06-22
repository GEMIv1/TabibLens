using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default);
        Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsTokenActiveAsync(string hashedToken, CancellationToken cancellationToken = default);
        void Revoke(RefreshToken refreshToken);
        Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task SoftDeleteExpiredAsync(CancellationToken cancellationToken = default);
    }
}
