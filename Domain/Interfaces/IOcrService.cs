namespace Domain.Interfaces
{
    public interface IOcrService
    {
        Task<OcrResult> ProcessImageAsync(byte[] imageData, string contentType, CancellationToken cancellationToken = default);
    }

    public record OcrResult(bool Success, string? RawText, string? ErrorMessage);
}
