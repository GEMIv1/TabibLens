using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TabibLens.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
        {
            return RedirectToPage("/Auth/Login");
        }
        
        return RedirectToPage("/Dashboard");
    }
}
