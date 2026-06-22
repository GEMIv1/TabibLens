using Application.DTOs;
using Domain.Enums;

namespace Application.Services.Abstraction
{
    public interface IPrescriptionService
    {
        Task<OcrResultDto> ScanPrescriptionAsync(OcrRequestDto request, CancellationToken cancellationToken = default);
        Task<PrescriptionDto?> GetPrescriptionByIdAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<PrescriptionWithMedicationsDto?> GetPrescriptionWithMedicationsAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PrescriptionSummaryDto>> GetUserPrescriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PrescriptionSummaryDto>> GetPrescriptionsByStatusAsync(Guid userId, PrescriptionStatus status, CancellationToken cancellationToken = default);
        Task<OcrResultDto?> GetPrescriptionResultAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<PrescriptionWithMedicationsDto> ParseMedicationsAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<bool> UpdatePrescriptionStatusAsync(Guid userId, Guid prescriptionId, PrescriptionStatus status, CancellationToken cancellationToken = default);
        Task DeletePrescriptionAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default);
    }
}