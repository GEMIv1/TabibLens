using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IChatSessionRepository : IRepository<ChatSession>
    {
        Task<ChatSession?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ChatSession?> GetByIdWithPrescriptionAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ChatSession?> GetByIdFullAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatSession>> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatSession>> GetByUserIdAndPrescriptionIdAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
