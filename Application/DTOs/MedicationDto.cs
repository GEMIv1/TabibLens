using Domain.Enums;

namespace Application.DTOs
{
    public class MedicationDto
    {
        public Guid Id { get; init; }
        public Guid PrescriptionId { get; init; }
        public required string DrugRawData { get; init; }
        public string? DrugNameNormalized { get; init; }
        public string? DoseRaw { get; init; }
        public string? FrequencyRaw { get; init; }
        public string? DurationRaw { get; init; }
        public string? StrengthRaw { get; init; }
        public double ConfidenceScore { get; init; }
        public DosageForm? DosageForm { get; init; }
    }
}
