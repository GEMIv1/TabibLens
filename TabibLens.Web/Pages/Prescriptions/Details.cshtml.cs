using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Prescriptions;

public class DetailsModel : PageModel
{
    private readonly ApiService _apiService;

    public DetailsModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public PrescriptionWithMedications Prescription { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        try
        {
            var p = await _apiService.GetPrescriptionWithMedicationsAsync(id);
            if (p == null) return NotFound();
            Prescription = p;
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Clear();
            TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
            return RedirectToPage("/Auth/Login");
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostParseAsync(Guid id)
    {
        try
        {
            await _apiService.ParseMedicationsAsync(id);
            TempData["SuccessMessage"] = "Medications parsed successfully.";
        }
        catch (ApiException ex)
        {
            TempData["ErrorMessage"] = "Failed to parse: " + ex.Message;
        }
        
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostChatAsync(Guid id)
    {
        try
        {
            var response = await _apiService.CreateSessionAsync("Prescription Q&A", id);
            if (response != null)
            {
                return RedirectToPage("/Chat/Index", new { sessionId = response.SessionId });
            }
        }
        catch (Exception)
        {
        }
        return RedirectToPage("/Chat/Index");
    }
}
