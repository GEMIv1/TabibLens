using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ChatMessageRepository : RepositoryBase<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<ChatMessage?> GetByIdWithSessionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _set.Include(cm => cm.ChatSession).FirstOrDefaultAsync(cm => cm.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<ChatMessage>> GetByRoleAsync(MessageRole role, CancellationToken cancellationToken = default)
        {
            return await _set.Where(cm => cm.Role == role).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChatMessage>> GetBySessionIdAndRoleAsync(Guid chatSessionId, MessageRole role, CancellationToken cancellationToken = default)
        {
            return await _set.Where(cm => cm.Role == role && cm.ChatSessionId == chatSessionId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(cm => cm.ChatSessionId == chatSessionId).ToListAsync(cancellationToken);
        }
    }
}
