using Domain.Enums;

namespace Application.DTOs
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public required string Content { get; set; }
        public MessageRole Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
