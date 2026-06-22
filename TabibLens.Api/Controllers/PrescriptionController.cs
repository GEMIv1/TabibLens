using Application.DTOs;
using Application.Services.Abstraction;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TabibLens.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PrescriptionController : BaseApiController
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanPrescription(IFormFile image, CancellationToken cancellationToken)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image provided." });

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancellationToken);

            var request = new OcrRequestDto
            {
                ImageData = ms.ToArray(),
                ContentType = image.ContentType,
                UserId = GetUserId()
            };

            var result = await _prescriptionService.ScanPrescriptionAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPrescriptions(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var prescriptions = await _prescriptionService.GetUserPrescriptionsAsync(userId, cancellationToken);
            return Ok(prescriptions);
        }

        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetPrescriptionById(Guid prescriptionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(userId, prescriptionId, cancellationToken);
            if (prescription == null) return NotFound();
            return Ok(prescription);
        }

        [HttpGet("{prescriptionId}/medications")]
        public async Task<IActionResult> GetPrescriptionWithMedications(Guid prescriptionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var prescription = await _prescriptionService.GetPrescriptionWithMedicationsAsync(userId, prescriptionId, cancellationToken);
            if (prescription == null) return NotFound();
            return Ok(prescription);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetPrescriptionsByStatus(PrescriptionStatus status, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var prescriptions = await _prescriptionService.GetPrescriptionsByStatusAsync(userId, status, cancellationToken);
            return Ok(prescriptions);
        }

        [HttpGet("{prescriptionId}/result")]
        public async Task<IActionResult> GetPrescriptionResult(Guid prescriptionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _prescriptionService.GetPrescriptionResultAsync(userId, prescriptionId, cancellationToken);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("{prescriptionId}/parse")]
        public async Task<IActionResult> ParseMedications(Guid prescriptionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _prescriptionService.ParseMedicationsAsync(userId, prescriptionId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{prescriptionId}/status")]
        public async Task<IActionResult> UpdatePrescriptionStatus(Guid prescriptionId, [FromBody] UpdateStatusRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var updated = await _prescriptionService.UpdatePrescriptionStatusAsync(userId, prescriptionId, request.Status, cancellationToken);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{prescriptionId}")]
        public async Task<IActionResult> DeletePrescription(Guid prescriptionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _prescriptionService.DeletePrescriptionAsync(userId, prescriptionId, cancellationToken);
            return NoContent();
        }
    }
}
