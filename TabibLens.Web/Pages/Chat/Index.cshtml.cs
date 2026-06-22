using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Chat;

public class IndexModel : PageModel
{
    private readonly ApiService _apiService;

    public IndexModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public List<ChatSession> Sessions { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid? SessionId { get; set; }
    
    public Guid? ActiveSessionId => SessionId;

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        try
        {
            Sessions = await _apiService.GetSessionsAsync();
            Sessions = Sessions.OrderByDescending(s => s.CreatedAt).ToList();

            if (SessionId.HasValue)
            {
                if (!Sessions.Any(s => s.Id == SessionId))
                {
                    return RedirectToPage("/Chat/Index");
                }

                Messages = await _apiService.GetMessagesAsync(SessionId.Value);
            }
            else if (Sessions.Any())
            {
                return RedirectToPage(new { sessionId = Sessions.First().Id });
            }
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
            return RedirectToPage("/Auth/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateSessionAsync(string title)
    {
        try
        {
            var response = await _apiService.CreateSessionAsync(title);
            if (response != null)
            {
                return RedirectToPage(new { sessionId = response.SessionId });
            }
        }
        catch
        {
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendMessageAsync(Guid sessionId, string message)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                await _apiService.SendMessageAsync(sessionId, message);
            }
        }
        catch
        {
        }
        return RedirectToPage(new { sessionId });
    }

    public async Task<IActionResult> OnPostDeleteSessionAsync(Guid sessionId)
    {
        try
        {
            await _apiService.DeleteSessionAsync(sessionId);
        }
        catch
        {
        }
        return RedirectToPage("/Chat/Index");
    }
}
