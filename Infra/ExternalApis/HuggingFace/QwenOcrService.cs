using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Infra.ExternalApis.HuggingFace
{
    public class QwenOcrService : IOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly HuggingFaceOptions _options;

        private const string MedicationExtractionPrompt =
            """
            Analyze this prescription image and extract ONLY the medications. 
            Ignore patient name, age, date, doctor info, and any non-medication details.
            
            Return a JSON array where each object has these fields:
            - "drugRawData": the medication name exactly as written on the prescription
            - "drugNameNormalized": the standard/generic drug name (or null if unsure)
            - "doseRaw": dosage as written (e.g. "500mg", "10ml")
            - "frequencyRaw": how often to take it as written (e.g. "twice daily", "every 8 hours")
            - "durationRaw": how long to take it as written (e.g. "7 days", "2 weeks")
            - "strengthRaw": strength as written (or null if not specified)
            - "confidenceScore": your confidence from 0.0 to 1.0
            - "dosageForm": one of: Unknown, Tablet, Capsule, Syrup, Injection, Cream, Drops, Other
            
            Return ONLY the JSON array, no markdown, no explanation, no extra text.
            """;

        public QwenOcrService(HttpClient httpClient, IOptions<HuggingFaceOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        public async Task<OcrResult> ProcessImageAsync(byte[] imageData, string contentType, CancellationToken cancellationToken = default)
        {
            var base64Image = Convert.ToBase64String(imageData);
            var dataUri = $"data:{contentType};base64,{base64Image}";

            var requestBody = new
            {
                model = _options.ModelId,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image_url", image_url = new { url = dataUri } },
                            new { type = "text", text = MedicationExtractionPrompt }
                        }
                    }
                },
                max_tokens = 2048
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_options.BaseUrl}/chat/completions";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new OcrResult(false, null, $"OCR model returned {response.StatusCode}: {responseBody}");
            }

            var parsedContent = ExtractContentFromResponse(responseBody);

            return parsedContent is not null
                ? new OcrResult(true, parsedContent, null)
                : new OcrResult(false, null, "Failed to parse model response.");
        }

        private static string? ExtractContentFromResponse(string responseBody)
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                return choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }

            return null;
        }
    }
}
