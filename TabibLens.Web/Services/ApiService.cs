using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TabibLens.Web.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _contextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiService(HttpClient http, IHttpContextAccessor contextAccessor)
    {
        _http = http;
        _contextAccessor = contextAccessor;
    }

    private void AttachToken()
    {
        var token = _contextAccessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }


    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var content = ToJson(request);
        var response = await _http.PostAsync("/api/auth/login", content);
        return await HandleResponse<AuthResponse>(response);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var content = ToJson(request);
        var response = await _http.PostAsync("/api/auth/register", content);
        return await HandleResponse<AuthResponse>(response);
    }

    public async Task LogoutAsync()
    {
        AttachToken();
        await _http.PostAsync("/api/auth/logout", null);
    }


    public async Task<List<PrescriptionSummary>> GetPrescriptionsAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("/api/prescription");
        return await HandleResponse<List<PrescriptionSummary>>(response) ?? new();
    }

    public async Task<PrescriptionDetail?> GetPrescriptionByIdAsync(Guid id)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/prescription/{id}");
        return await HandleResponse<PrescriptionDetail>(response);
    }

    public async Task<PrescriptionWithMedications?> GetPrescriptionWithMedicationsAsync(Guid id)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/prescription/{id}/medications");
        return await HandleResponse<PrescriptionWithMedications>(response);
    }

    public async Task<List<PrescriptionSummary>> GetPrescriptionsByStatusAsync(string status)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/prescription/status/{status}");
        return await HandleResponse<List<PrescriptionSummary>>(response) ?? new();
    }

    public async Task<OcrResult?> ScanPrescriptionAsync(Stream imageStream, string fileName, string contentType)
    {
        AttachToken();
        using var form = new MultipartFormDataContent();
        var streamContent = new StreamContent(imageStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "image", fileName);

        var response = await _http.PostAsync("/api/prescription/scan", form);
        return await HandleResponse<OcrResult>(response);
    }

    public async Task<PrescriptionWithMedications?> ParseMedicationsAsync(Guid prescriptionId)
    {
        AttachToken();
        var response = await _http.PostAsync($"/api/prescription/{prescriptionId}/parse", null);
        return await HandleResponse<PrescriptionWithMedications>(response);
    }

    public async Task<OcrResult?> GetPrescriptionResultAsync(Guid id)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/prescription/{id}/result");
        return await HandleResponse<OcrResult>(response);
    }

    public async Task<bool> UpdatePrescriptionStatusAsync(Guid id, string status)
    {
        AttachToken();
        var content = ToJson(new { status });
        var response = await _http.PatchAsync($"/api/prescription/{id}/status", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePrescriptionAsync(Guid id)
    {
        AttachToken();
        var response = await _http.DeleteAsync($"/api/prescription/{id}");
        return response.IsSuccessStatusCode;
    }


    public async Task<List<ChatSession>> GetSessionsAsync()
    {
        AttachToken();
        var response = await _http.GetAsync("/api/chat/sessions");
        return await HandleResponse<List<ChatSession>>(response) ?? new();
    }

    public async Task<CreateSessionResponse?> CreateSessionAsync(string title, Guid? prescriptionId = null)
    {
        AttachToken();
        var content = ToJson(new { title, prescriptionId });
        var response = await _http.PostAsync("/api/chat/sessions", content);
        return await HandleResponse<CreateSessionResponse>(response);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid sessionId)
    {
        AttachToken();
        var response = await _http.GetAsync($"/api/chat/sessions/{sessionId}/messages");
        return await HandleResponse<List<ChatMessage>>(response) ?? new();
    }

    public async Task<ChatResponse?> SendMessageAsync(Guid sessionId, string message)
    {
        AttachToken();
        var content = ToJson(new { message });
        var response = await _http.PostAsync($"/api/chat/sessions/{sessionId}/messages", content);
        return await HandleResponse<ChatResponse>(response);
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        AttachToken();
        var response = await _http.DeleteAsync($"/api/chat/sessions/{sessionId}");
        return response.IsSuccessStatusCode;
    }


    private static StringContent ToJson(object obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(response.StatusCode, body);
        }

        if (string.IsNullOrWhiteSpace(body))
            return default;

        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }
}


public class ApiException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string ResponseBody { get; }

    public ApiException(System.Net.HttpStatusCode statusCode, string responseBody)
        : base($"API returned {(int)statusCode}: {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? PhoneNumber { get; set; }
}

public class AuthResponse
{
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public string AccessToken { get; set; } = "";
}

public class PrescriptionSummary
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = "";
    public string? FailureReason { get; set; }
    public DateTimeOffset? OcrProcessedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class PrescriptionDetail
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? OcrRawData { get; set; }
    public string Status { get; set; } = "";
    public string? FailureReason { get; set; }
    public DateTimeOffset? OcrProcessedAt { get; set; }
    public List<MedicationItem> Medications { get; set; } = new();
}

public class PrescriptionWithMedications
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? OcrRawData { get; set; }
    public string Status { get; set; } = "";
    public string? FailureReason { get; set; }
    public DateTimeOffset? OcrProcessedAt { get; set; }
    public List<MedicationItem> Medications { get; set; } = new();
}

public class MedicationItem
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public string DrugRawData { get; set; } = "";
    public string? DrugNameNormalized { get; set; }
    public string? DoseRaw { get; set; }
    public string? FrequencyRaw { get; set; }
    public string? DurationRaw { get; set; }
    public string? StrengthRaw { get; set; }
    public double ConfidenceScore { get; set; }
    public string? DosageForm { get; set; }
}

public class OcrResult
{
    public bool? Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawText { get; set; }
    public PrescriptionDetail? Prescription { get; set; }
}

public class ChatSession
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public Guid? PrescriptionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class CreateSessionResponse
{
    public Guid SessionId { get; set; }
}

public class ChatMessage
{
    public Guid Id { get; set; }
    public string Content { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public class ChatResponse
{
    public Guid MessageId { get; set; }
    public string Content { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
}
