using Domain.Entities;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
        {
            return await _set.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.HashedToken == hashedToken, cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(rt => rt.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTimeOffset.UtcNow).ToListAsync(cancellationToken);
        }

        public async Task<bool> IsTokenActiveAsync(string hashedToken, CancellationToken cancellationToken = default)
        {
            return await _set.AnyAsync(rt => rt.HashedToken == hashedToken && rt.RevokedAt == null && rt.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);
        }

        public void Revoke(RefreshToken refreshToken)
        {
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;
            _set.Update(refreshToken);
        }

        public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            await _set.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(rt => rt.RevokedAt, now)
                    .SetProperty(rt => rt.UpdatedAt, now),
                    cancellationToken);
        }

        public async Task SoftDeleteExpiredAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            await _set
                .Where(rt => rt.ExpiresAt <= now && !rt.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(rt => rt.IsDeleted, true)
                    .SetProperty(rt => rt.DeletedAt, now),
                    cancellationToken);
        }
    }
}
