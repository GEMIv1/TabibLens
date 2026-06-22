namespace Application.DTOs
{
    public class ChatResponseDto
    {
        public Guid MessageId { get; set; }
        public required string Content { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
