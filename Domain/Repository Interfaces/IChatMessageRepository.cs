using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        Task<ChatMessage?> GetByIdWithSessionAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessage>> GetBySessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessage>> GetByRoleAsync(MessageRole role, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessage>> GetBySessionIdAndRoleAsync(Guid chatSessionId, MessageRole role, CancellationToken cancellationToken = default);
    }
}
