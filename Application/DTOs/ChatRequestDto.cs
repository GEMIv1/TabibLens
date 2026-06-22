using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ChatRequestDto
    {
        [Required]
        public required string Message { get; set; }
    }
}
