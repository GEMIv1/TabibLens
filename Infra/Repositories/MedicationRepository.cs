using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class MedicationRepository : RepositoryBase<Medication>, IMedicationRepository
    {
        public MedicationRepository(AppDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Medication>> GetByDosageFormAsync(DosageForm dosageForm, CancellationToken cancellationToken = default)
        {
            return await _set.Where(md => md.DosageForm == dosageForm).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Medication>> GetByDrugNameAsync(string drugNameNormalized, CancellationToken cancellationToken = default)
        {
            return await _set.Where(md => md.DrugNameNormalized == drugNameNormalized).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Medication>> GetByPrescriptionIdAsync(Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            return await _set.Where(md => md.PrescriptionId == prescriptionId).ToListAsync(cancellationToken);
        }
    }
}
