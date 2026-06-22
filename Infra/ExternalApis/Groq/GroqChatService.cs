using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Infra.ExternalApis.Groq
{
    public class GroqChatService : IChatAiService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqOptions _options;

        private const string BaseSystemPrompt = "You are a helpful pharmaceutical assistant. " +
    "Answer questions about medications including side effects, drug interactions, " +
    "replacements/alternatives, storage, and usage instructions. " +
    "Always advise consulting a healthcare professional for medical decisions. " +
    "Keep your answers clear, concise, and evidence-based. " +
    "Detect the language of the user's message: if they write in Arabic, respond in Arabic; otherwise, respond in English.";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GroqChatService(HttpClient httpClient, IOptions<GroqOptions> groqOptions)
        {
            _httpClient = httpClient;
            _options = groqOptions.Value;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        public async Task<ChatAiResponse> GetCompletionAsync(IEnumerable<AiChatMessage> messages, string? systemPrompt = null, CancellationToken cancellationToken = default)
        {
            var allMessages = new List<object>();

            var combinedSystemPrompt = string.IsNullOrWhiteSpace(systemPrompt)
                ? BaseSystemPrompt
                : $"{BaseSystemPrompt}\n\n{systemPrompt}";

            allMessages.Add(new { role = "system", content = combinedSystemPrompt });

            foreach (var msg in messages)
            {
                allMessages.Add(new { role = msg.Role, content = msg.Content });
            }

            var requestBody = new
            {
                model = _options.Model,
                messages = allMessages,
                temperature = _options.Temperature,
                max_tokens = _options.MaxTokens
            };

            var json = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_options.BaseUrl}/chat/completions";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ChatAiResponse(false, null, $"Groq API returned {response.StatusCode}: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                return new ChatAiResponse(false, null, "Groq API returned no choices.");
            }

            var assistantContent = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return new ChatAiResponse(true, assistantContent, null);
        }
    }
}
