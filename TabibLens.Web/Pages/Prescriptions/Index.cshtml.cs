using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Prescriptions;

public class IndexModel : PageModel
{
    private readonly ApiService _apiService;

    public IndexModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public List<PrescriptionSummary> Prescriptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }
    
    public string CurrentStatus => Status ?? "";

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        try
        {
            if (string.IsNullOrEmpty(Status))
            {
                Prescriptions = await _apiService.GetPrescriptionsAsync();
            }
            else
            {
                Prescriptions = await _apiService.GetPrescriptionsByStatusAsync(Status);
            }

            Prescriptions = Prescriptions.OrderByDescending(p => p.CreatedAt).ToList();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
            return RedirectToPage("/Auth/Login");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            await _apiService.DeletePrescriptionAsync(id);
            TempData["SuccessMessage"] = "Prescription deleted successfully.";
        }
        catch
        {
        }
        return RedirectToPage();
    }
}
