using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateSessionRequestDto
    {
        [Required]
        public required string Title { get; set; }
        public Guid? PrescriptionId { get; set; }
    }
}
