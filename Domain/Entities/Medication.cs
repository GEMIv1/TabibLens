using Domain.Entities.Abstractions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Medication: BaseEntity
    {
        public Guid PrescriptionId { get; set; }
        public required string DrugRawData {  get; set; }
        public string? DrugNameNormalized { get; set; }
        public string? DoseRaw {  get; set; }
        public string? FrequencyRaw {  get; set; }
        public string? DurationRaw {  get; set; }
        public string? StrengthRaw {  get; set; }
        public double ConfidenceScore {  get; set; }
        public DosageForm? DosageForm { get; set; }
        public Prescription? Prescription { get; set; }
    }
}
