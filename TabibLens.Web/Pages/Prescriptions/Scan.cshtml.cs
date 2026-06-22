using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Prescriptions;

public class ScanModel : PageModel
{
    private readonly ApiService _apiService;

    public ScanModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            ErrorMessage = "Please select an image file to scan.";
            return Page();
        }

        try
        {
            using var stream = image.OpenReadStream();
            var result = await _apiService.ScanPrescriptionAsync(stream, image.FileName, image.ContentType);

            if (result != null && result.Success == true && result.Prescription != null)
            {
                TempData["SuccessMessage"] = "Prescription scanned successfully!";
                return RedirectToPage("./Details", new { id = result.Prescription.Id });
            }

            ErrorMessage = result?.ErrorMessage ?? "Failed to process the prescription. The OCR service might be unavailable.";
            return Page();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.ResponseBody;
            return Page();
        }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred during processing. Please try again.";
            return Page();
        }
    }
}
