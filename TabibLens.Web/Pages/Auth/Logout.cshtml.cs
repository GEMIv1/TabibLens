using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly ApiService _apiService;

    public LogoutModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _apiService.LogoutAsync();
        }
        catch
        {
        }
        finally
        {
            HttpContext.Session.Clear();
        }

        return RedirectToPage("/Auth/Login");
    }
}
