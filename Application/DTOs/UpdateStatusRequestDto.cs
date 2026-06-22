using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs
{
    public class UpdateStatusRequestDto
    {
        [Required]
        public PrescriptionStatus Status { get; set; }
    }
}
