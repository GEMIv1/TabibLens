using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PrescriptionDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string? OcrRawData { get; init; }
        public PrescriptionStatus Status { get; init; }
        public string? FailureReason { get; init; }
        public DateTimeOffset? OcrProcessedAt { get; init; }
        public ICollection<MedicationDto> Medications { get; init; } = new List<MedicationDto>();
    }
}
