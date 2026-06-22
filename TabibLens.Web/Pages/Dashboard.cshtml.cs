using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages;

public class DashboardModel : PageModel
{
    private readonly ApiService _apiService;

    public DashboardModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public string UserName { get; set; } = "";
    public int TotalCount { get; set; }
    public int ParsedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int FailedCount { get; set; }
    public List<PrescriptionSummary> RecentPrescriptions { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
        {
            return RedirectToPage("/Auth/Login");
        }

        UserName = HttpContext.Session.GetString("UserName") ?? "Doctor";

        try
        {
            var prescriptions = await _apiService.GetPrescriptionsAsync();
            
            TotalCount = prescriptions.Count;
            ParsedCount = prescriptions.Count(p => p.Status == "Parsed" || p.Status == "PartiallyParsed");
            ProcessingCount = prescriptions.Count(p => p.Status == "Uploaded" || p.Status == "OcrProcessing");
            FailedCount = prescriptions.Count(p => p.Status == "Failed");

            RecentPrescriptions = prescriptions
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
            return RedirectToPage("/Auth/Login");
        }
        catch
        {
        }

        return Page();
    }
}
