using Domain.Entities;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ChatSessionRepository : RepositoryBase<ChatSession>, IChatSessionRepository
    {
        public ChatSessionRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<bool> ExistsByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.AnyAsync(cs => cs.Id == id && cs.UserId == userId, cancellationToken);
        }

        public async Task<ChatSession?> GetByIdFullAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set
                .Include(cs => cs.Messages)
                .Include(cs => cs.Prescription)
                    .ThenInclude(p => p.Medications)
                .FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
        }

        public async Task<ChatSession?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(cs => cs.Messages).FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
        }

        public async Task<ChatSession?> GetByIdWithPrescriptionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(cs => cs.Prescription).FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ChatSession>> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(cs => cs.PrescriptionId == prescriptionId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _set.Include(cs => cs.Messages).Where(cs => cs.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChatSession>> GetByUserIdAndPrescriptionIdAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(cs => cs.UserId == userId && cs.PrescriptionId == prescriptionId).ToListAsync(cancellationToken);
        }
    }
}
