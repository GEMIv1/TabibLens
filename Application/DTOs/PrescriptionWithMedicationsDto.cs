using Domain.Enums;

namespace Application.DTOs
{
    public class PrescriptionWithMedicationsDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string? OcrRawData { get; init; }
        public PrescriptionStatus Status { get; init; }
        public DateTimeOffset? OcrProcessedAt { get; init; }
        public ICollection<MedicationDto> Medications { get; init; } = new List<MedicationDto>();
    }
}
