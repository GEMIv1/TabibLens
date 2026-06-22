using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IMedicationRepository : IRepository<Medication>
    {
        Task<IEnumerable<Medication>> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Medication>> GetByDrugNameAsync(string drugNameNormalized, CancellationToken cancellationToken = default);
        Task<IEnumerable<Medication>> GetByDosageFormAsync(DosageForm dosageForm, CancellationToken cancellationToken = default);
    }
}
