namespace Application.DTOs
{
    public class ChatSessionDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public Guid? PrescriptionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int MessageCount { get; set; }
    }
}
