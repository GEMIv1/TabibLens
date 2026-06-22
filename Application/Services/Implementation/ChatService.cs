using System.Text;
using Application.DTOs;
using Application.Services.Abstraction;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services.Implementation
{
    public class ChatService : IChatService
    {
        private readonly IChatSessionRepository _sessionRepository;
        private readonly IChatMessageRepository _messageRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IChatAiService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(
            IChatSessionRepository sessionRepository,
            IChatMessageRepository messageRepository,
            IPrescriptionRepository prescriptionRepository,
            IChatAiService aiService,
            IUnitOfWork unitOfWork)
        {
            _sessionRepository = sessionRepository;
            _messageRepository = messageRepository;
            _prescriptionRepository = prescriptionRepository;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateSessionAsync(Guid userId, string title, Guid? prescriptionId = null, CancellationToken cancellationToken = default)
        {
            if (prescriptionId.HasValue)
            {
                var prescriptionExists = await _prescriptionRepository.ExistsByIdAndUserIdAsync(prescriptionId.Value, userId, cancellationToken);
                if (!prescriptionExists)
                    throw new InvalidOperationException("Prescription not found or does not belong to the user.");
            }

            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                PrescriptionId = prescriptionId,
                CreatedAt = DateTimeOffset.UtcNow,
                User = null!
            };

            await _sessionRepository.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return session.Id;
        }

        public async Task<ChatResponseDto> SendMessageAsync(Guid userId, Guid sessionId, ChatRequestDto request, CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetByIdFullAsync(sessionId, cancellationToken)?? throw new InvalidOperationException("Chat session not found.");

            if (session.UserId != userId)
                throw new InvalidOperationException("Chat session not found.");

            var userMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = sessionId,
                Content = request.Message,
                Role = MessageRole.User,
                CreatedAt = DateTimeOffset.UtcNow,
                ChatSession = session
            };

            await _messageRepository.AddAsync(userMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            string? medicationContext = BuildMedicationContext(session.Prescription);

            var aiMessages = session.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AiChatMessage(
                    m.Role == MessageRole.User ? "user" : "assistant",
                    m.Content))
                .ToList();

            // User message is already included in session.Messages after SaveChangesAsync above

            var aiResponse = await _aiService.GetCompletionAsync(aiMessages, medicationContext, cancellationToken);

            if (!aiResponse.Success)
                throw new InvalidOperationException($"AI service failed: {aiResponse.ErrorMessage}");

            var assistantContent = aiResponse.Content ?? "I'm sorry, I couldn't generate a response.";

            var assistantMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = sessionId,
                Content = assistantContent,
                Role = MessageRole.Assistant,
                CreatedAt = DateTimeOffset.UtcNow,
                ChatSession = session
            };

            await _messageRepository.AddAsync(assistantMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ChatResponseDto
            {
                MessageId = assistantMessage.Id,
                Content = assistantContent,
                Timestamp = assistantMessage.CreatedAt
            };
        }

        public async Task<IEnumerable<ChatMessageDto>> GetSessionMessagesAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            var ownershipValid = await _sessionRepository.ExistsByIdAndUserIdAsync(sessionId, userId, cancellationToken);
            if (!ownershipValid)
                throw new InvalidOperationException("Chat session not found.");

            var messages = await _messageRepository.GetBySessionIdAsync(sessionId, cancellationToken);

            return messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    Role = m.Role,
                    CreatedAt = m.CreatedAt
                });
        }

        public async Task<IEnumerable<ChatSessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var sessions = await _sessionRepository.GetByUserIdAsync(userId, cancellationToken);

            return sessions
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ChatSessionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    PrescriptionId = s.PrescriptionId,
                    CreatedAt = s.CreatedAt,
                    MessageCount = s.Messages.Count
                });
        }

        public async Task DeleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
                ?? throw new InvalidOperationException("Chat session not found.");

            if (session.UserId != userId)
                throw new InvalidOperationException("Chat session not found.");

            _sessionRepository.SoftDelete(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }


        private static string? BuildMedicationContext(Prescription? prescription)
        {
            if (prescription == null || !prescription.Medications.Any())
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("The patient has the following prescription medications:");
            sb.AppendLine();

            int index = 1;
            foreach (var med in prescription.Medications)
            {
                var name = med.DrugNameNormalized ?? med.DrugRawData;
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(med.DoseRaw))
                    parts.Add($"Dose: {med.DoseRaw}");
                if (!string.IsNullOrWhiteSpace(med.StrengthRaw))
                    parts.Add($"Strength: {med.StrengthRaw}");
                if (!string.IsNullOrWhiteSpace(med.FrequencyRaw))
                    parts.Add($"Frequency: {med.FrequencyRaw}");
                if (!string.IsNullOrWhiteSpace(med.DurationRaw))
                    parts.Add($"Duration: {med.DurationRaw}");
                if (med.DosageForm.HasValue)
                    parts.Add($"Form: {med.DosageForm.Value}");

                var details = parts.Count > 0 ? $" — {string.Join(", ", parts)}" : "";
                sb.AppendLine($"{index}. {name}{details}");
                index++;
            }

            return sb.ToString().TrimEnd();
        }
    }
}
