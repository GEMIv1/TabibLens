using Application.DTOs;
using Application.Services.Abstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TabibLens.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ChatController : BaseApiController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var sessionId = await _chatService.CreateSessionAsync(userId, request.Title, request.PrescriptionId, cancellationToken);
            return CreatedAtAction(nameof(GetSessionMessages), new { sessionId }, new { sessionId });
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetUserSessions(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var sessions = await _chatService.GetUserSessionsAsync(userId, cancellationToken);
            return Ok(sessions);
        }

        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetSessionMessages(Guid sessionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var messages = await _chatService.GetSessionMessagesAsync(userId, sessionId, cancellationToken);
            return Ok(messages);
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] ChatRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var response = await _chatService.SendMessageAsync(userId, sessionId, request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _chatService.DeleteSessionAsync(userId, sessionId, cancellationToken);
            return NoContent();
        }
    }
}
