using System.Text.Json;
using Application.DTOs;
using Application.Services.Abstraction;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;


namespace Application.Services.Implementation
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IOcrService _ocrService;
        private readonly IPrescriptionRepository _repository;
        private readonly IMedicationRepository _medicationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PrescriptionService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PrescriptionService(
            IOcrService ocrService,
            IPrescriptionRepository repository,
            IMedicationRepository medicationRepository,
            IUnitOfWork unitOfWork,
            ILogger<PrescriptionService> logger)
        {
            _ocrService = ocrService;
            _repository = repository;
            _medicationRepository = medicationRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OcrResultDto> ScanPrescriptionAsync(OcrRequestDto request, CancellationToken cancellationToken = default)
        {
            var ocrResult = await _ocrService.ProcessImageAsync(request.ImageData, request.ContentType, cancellationToken);

            var prescription = new Prescription
            {
                UserId = request.UserId,
                OcrRawData = ocrResult.RawText,
                OcrProcessedAt = DateTimeOffset.UtcNow,
                Status = ocrResult.Success ? PrescriptionStatus.OcrProcessing : PrescriptionStatus.Failed,
                FailureReason = ocrResult.Success ? null : ocrResult.ErrorMessage,
                User = null!
            };

            await _repository.AddAsync(prescription, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!ocrResult.Success)
            {
                return new OcrResultDto
                {
                    Success = false,
                    RawText = null,
                    ErrorMessage = ocrResult.ErrorMessage
                };
            }

            var medications = await ParseAndStoreMedicationsAsync(prescription, cancellationToken);

            prescription.Status = medications.Any() ? PrescriptionStatus.Parsed : PrescriptionStatus.PartiallyParsed;
            _repository.Update(prescription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new OcrResultDto
            {
                Success = true,
                RawText = ocrResult.RawText,
                ErrorMessage = null,
                Prescription = new PrescriptionDto
                {
                    Id = prescription.Id,
                    UserId = prescription.UserId,
                    OcrRawData = prescription.OcrRawData,
                    Status = prescription.Status,
                    FailureReason = prescription.FailureReason,
                    OcrProcessedAt = prescription.OcrProcessedAt,
                    Medications = medications.Select(MapToMedicationDto).ToList()
                }
            };
        }

        public async Task<PrescriptionDto?> GetPrescriptionByIdAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            var prescription = await _repository.GetByIdAsync(prescriptionId, cancellationToken);

            if (prescription is null || prescription.UserId != userId) return null;

            return MapToPrescriptionDto(prescription);
        }

        public async Task<PrescriptionWithMedicationsDto?> GetPrescriptionWithMedicationsAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            var prescription = await _repository.GetByIdWithMedicationsAsync(prescriptionId, cancellationToken);

            if (prescription is null || prescription.UserId != userId) return null;

            return new PrescriptionWithMedicationsDto
            {
                Id = prescription.Id,
                UserId = prescription.UserId,
                OcrRawData = prescription.OcrRawData,
                Status = prescription.Status,
                OcrProcessedAt = prescription.OcrProcessedAt,
                Medications = prescription.Medications.Select(MapToMedicationDto).ToList()
            };
        }

        public async Task<IEnumerable<PrescriptionSummaryDto>> GetUserPrescriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var prescriptions = await _repository.GetByUserIdAsync(userId, cancellationToken);

            if (!prescriptions.Any()) return Enumerable.Empty<PrescriptionSummaryDto>();

            return prescriptions.Select(MapToSummaryDto);
        }

        public async Task<IEnumerable<PrescriptionSummaryDto>> GetPrescriptionsByStatusAsync(Guid userId, PrescriptionStatus status, CancellationToken cancellationToken = default)
        {
            var prescriptions = await _repository.GetByUserIdAndStatusAsync(status, userId, cancellationToken);

            if (!prescriptions.Any()) return Enumerable.Empty<PrescriptionSummaryDto>();

            return prescriptions.Select(MapToSummaryDto);
        }

        public async Task<OcrResultDto?> GetPrescriptionResultAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            var prescription = await _repository.GetByIdAsync(prescriptionId, cancellationToken);

            if (prescription is null || prescription.UserId != userId) return null;

            return new OcrResultDto
            {
                Success = prescription.Status != PrescriptionStatus.Failed,
                RawText = prescription.OcrRawData,
                ErrorMessage = prescription.FailureReason,
                Prescription = MapToPrescriptionDto(prescription)
            };
        }

        public async Task<PrescriptionWithMedicationsDto> ParseMedicationsAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            var prescription = await _repository.GetByIdWithMedicationsAsync(prescriptionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Prescription with ID {prescriptionId} not found.");

            if (prescription.UserId != userId)
                throw new KeyNotFoundException($"Prescription with ID {prescriptionId} not found.");

            if (string.IsNullOrWhiteSpace(prescription.OcrRawData))
                throw new InvalidOperationException("No OCR data available to parse.");

            // Soft-delete existing medications before re-parsing
            foreach (var med in prescription.Medications.ToList())
            {
                _medicationRepository.SoftDelete(med);
            }

            // Re-parse from raw OCR data
            var medications = await ParseAndStoreMedicationsAsync(prescription, cancellationToken);

            prescription.Status = medications.Any() ? PrescriptionStatus.Parsed : PrescriptionStatus.PartiallyParsed;
            _repository.Update(prescription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new PrescriptionWithMedicationsDto
            {
                Id = prescription.Id,
                UserId = prescription.UserId,
                OcrRawData = prescription.OcrRawData,
                Status = prescription.Status,
                OcrProcessedAt = prescription.OcrProcessedAt,
                Medications = medications.Select(MapToMedicationDto).ToList()
            };
        }

        public async Task<bool> UpdatePrescriptionStatusAsync(Guid userId, Guid prescriptionId, PrescriptionStatus status, CancellationToken cancellationToken = default)
        {
            var prescription = await _repository.GetByIdAsync(prescriptionId, cancellationToken);
            if (prescription is null || prescription.UserId != userId)
                return false;

            ValidateStatusTransition(prescription.Status, status);

            return await _repository.UpdateStatusAsync(prescriptionId, status, cancellationToken);
        }

        public async Task DeletePrescriptionAsync(Guid userId, Guid prescriptionId, CancellationToken cancellationToken = default)
        {
            var prescription = await _repository.GetByIdAsync(prescriptionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Prescription with ID {prescriptionId} not found.");

            if (prescription.UserId != userId)
                throw new KeyNotFoundException($"Prescription with ID {prescriptionId} not found.");

            _repository.SoftDelete(prescription);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static PrescriptionDto MapToPrescriptionDto(Prescription p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            OcrRawData = p.OcrRawData,
            Status = p.Status,
            FailureReason = p.FailureReason,
            OcrProcessedAt = p.OcrProcessedAt
        };

        private static PrescriptionSummaryDto MapToSummaryDto(Prescription p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            Status = p.Status,
            FailureReason = p.FailureReason,
            OcrProcessedAt = p.OcrProcessedAt,
            CreatedAt = p.CreatedAt
        };

        private static MedicationDto MapToMedicationDto(Medication m) => new()
        {
            Id = m.Id,
            PrescriptionId = m.PrescriptionId,
            DrugRawData = m.DrugRawData,
            DrugNameNormalized = m.DrugNameNormalized,
            DoseRaw = m.DoseRaw,
            FrequencyRaw = m.FrequencyRaw,
            DurationRaw = m.DurationRaw,
            StrengthRaw = m.StrengthRaw,
            ConfidenceScore = m.ConfidenceScore,
            DosageForm = m.DosageForm
        };

        private async Task<List<Medication>> ParseAndStoreMedicationsAsync(Prescription prescription, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(prescription.OcrRawData))
                return new List<Medication>();

            List<ParsedMedicationJson>? parsedMedications;
            try
            {
                parsedMedications = JsonSerializer.Deserialize<List<ParsedMedicationJson>>(
                    prescription.OcrRawData,
                    JsonOptions
                ) ?? new List<ParsedMedicationJson>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse OCR JSON for prescription {PrescriptionId}", prescription.Id);
                return new List<Medication>();
            }

            var medications = parsedMedications.Select(pm => new Medication
            {
                PrescriptionId = prescription.Id,
                DrugRawData = pm.DrugRawData ?? "Unknown",
                DrugNameNormalized = pm.DrugNameNormalized,
                DoseRaw = pm.DoseRaw,
                FrequencyRaw = pm.FrequencyRaw,
                DurationRaw = pm.DurationRaw,
                StrengthRaw = pm.StrengthRaw,
                ConfidenceScore = pm.ConfidenceScore,
                DosageForm = Enum.TryParse<DosageForm>(pm.DosageForm, true, out var form) ? form : DosageForm.Unknown,
                Prescription = prescription
            }).ToList();

            foreach (var med in medications)
            {
                await _medicationRepository.AddAsync(med, cancellationToken);
            }

            return medications;
        }

        private static void ValidateStatusTransition(PrescriptionStatus current, PrescriptionStatus next)
        {
            var isValid = (current, next) switch
            {
                (PrescriptionStatus.Uploaded, PrescriptionStatus.OcrProcessing) => true,
                (PrescriptionStatus.Uploaded, PrescriptionStatus.Failed) => true,
                (PrescriptionStatus.OcrProcessing, PrescriptionStatus.Parsed) => true,
                (PrescriptionStatus.OcrProcessing, PrescriptionStatus.PartiallyParsed) => true,
                (PrescriptionStatus.OcrProcessing, PrescriptionStatus.Failed) => true,
                (PrescriptionStatus.PartiallyParsed, PrescriptionStatus.Parsed) => true,
                (PrescriptionStatus.PartiallyParsed, PrescriptionStatus.Failed) => true,
                _ => false
            };

            if (!isValid)
                throw new InvalidOperationException($"Cannot transition prescription status from '{current}' to '{next}'.");
        }

        private sealed class ParsedMedicationJson
        {
            public string? DrugRawData { get; set; }
            public string? DrugNameNormalized { get; set; }
            public string? DoseRaw { get; set; }
            public string? FrequencyRaw { get; set; }
            public string? DurationRaw { get; set; }
            public string? StrengthRaw { get; set; }
            public double ConfidenceScore { get; set; }
            public string? DosageForm { get; set; }
        }
    }
}
