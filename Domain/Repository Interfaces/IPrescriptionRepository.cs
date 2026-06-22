using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IPrescriptionRepository : IRepository<Prescription>
    {
        Task<Prescription?> GetByIdWithMedicationsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Prescription?> GetByIdWithChatSessionAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Prescription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Prescription>> GetByStatusAsync(PrescriptionStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Prescription>> GetByUserIdAndStatusAsync(PrescriptionStatus status, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateStatusAsync(Guid id, PrescriptionStatus status, CancellationToken cancellationToken = default);
        Task<bool> MarkOcrFailedAsync(Guid id, string failureReason, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
