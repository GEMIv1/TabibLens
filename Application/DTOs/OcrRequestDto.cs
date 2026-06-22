namespace Application.DTOs
{
    public class OcrRequestDto
    {
        public required byte[] ImageData { get; init; }
        public required string ContentType { get; init; }
        public required Guid UserId { get; init; }
    }
}
