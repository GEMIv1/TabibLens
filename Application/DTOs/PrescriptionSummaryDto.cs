using Domain.Enums;

namespace Application.DTOs
{
    public class PrescriptionSummaryDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public PrescriptionStatus Status { get; init; }
        public string? FailureReason { get; init; }
        public DateTimeOffset? OcrProcessedAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
