using Application.DTOs;

namespace Application.Services.Abstraction
{
    public interface IChatService
    {
        Task<Guid> CreateSessionAsync(Guid userId, string title, Guid? prescriptionId = null, CancellationToken cancellationToken = default);

        Task<ChatResponseDto> SendMessageAsync(Guid userId, Guid sessionId, ChatRequestDto request, CancellationToken cancellationToken = default);

        Task<IEnumerable<ChatMessageDto>> GetSessionMessagesAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);

        Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

        Task DeleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    }
}
