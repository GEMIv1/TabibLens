namespace Domain.Interfaces
{
    public interface IChatAiService
    {
        Task<ChatAiResponse> GetCompletionAsync(IEnumerable<AiChatMessage> messages, string? systemPrompt = null, CancellationToken cancellationToken = default);
    }

    public record AiChatMessage(string Role, string Content);

    public record ChatAiResponse(bool Success, string? Content, string? ErrorMessage);
}
