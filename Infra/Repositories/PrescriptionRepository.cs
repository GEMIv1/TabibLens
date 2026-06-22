using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class PrescriptionRepository : RepositoryBase<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<bool> ExistsByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.AnyAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
        }

        public async Task<Prescription?> GetByIdWithChatSessionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(p => p.ChatSessions).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Prescription?> GetByIdWithMedicationsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(p => p.Medications).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Prescription>> GetByStatusAsync(PrescriptionStatus status, CancellationToken cancellationToken = default)
        {
            return await _set.Where(p => p.Status == status).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Prescription>> GetByUserIdAndStatusAsync(PrescriptionStatus status, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(p => p.Status == status && p.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Prescription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(p => p.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<bool> MarkOcrFailedAsync(Guid id, string failureReason, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var affectedRows = await _set.Where(p => p.Id == id && !p.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, PrescriptionStatus.Failed)
                    .SetProperty(p => p.FailureReason, failureReason)
                    .SetProperty(p => p.UpdatedAt, now),
                    cancellationToken
                );
            return affectedRows > 0;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, PrescriptionStatus status, CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;
            var affectedRows = await _set.Where(p => p.Id == id && !p.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, status)
                    .SetProperty(p => p.UpdatedAt, now),
                    cancellationToken
                );
            return affectedRows > 0;
        }
    }
}
