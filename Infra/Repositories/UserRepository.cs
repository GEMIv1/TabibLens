using Domain.Entities;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _set.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _set.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _set.AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _set.AnyAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<User?> GetWithRefreshTokensAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetWithPrescriptionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(u => u.Prescriptions).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetWithChatSessionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(u => u.ChatSessions).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailWithRefreshTokensAsync(string email, CancellationToken cancellationToken = default)
        {
           return await _set.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
    }
}
