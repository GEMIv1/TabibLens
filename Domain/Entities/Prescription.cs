using Domain.Entities.Abstractions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Prescription: BaseEntity
    {
        public Guid UserId { get; set; }
        public string? OcrRawData {  get; set; }
        public PrescriptionStatus Status {  get; set; }
        public string? FailureReason {  get; set; }
        public DateTimeOffset? OcrProcessedAt {  get; set; }
        public User? User { get; set; }
        public ICollection<Medication> Medications { get; set; } = new List<Medication>();
        public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    }
}
