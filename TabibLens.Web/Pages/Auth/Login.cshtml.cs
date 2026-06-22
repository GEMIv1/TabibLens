using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly ApiService _apiService;

    public LoginModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "";
    }

    public IActionResult OnGet()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
        {
            return RedirectToPage("/Dashboard");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new LoginRequest
            {
                Email = Input.Email,
                Password = Input.Password
            };

            var response = await _apiService.LoginAsync(request);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                HttpContext.Session.SetString("AccessToken", response.AccessToken);
                HttpContext.Session.SetString("UserName", response.UserName);
                HttpContext.Session.SetString("UserEmail", response.Email);

                return RedirectToPage("/Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.ResponseBody.Contains("message") ? 
                System.Text.Json.JsonDocument.Parse(ex.ResponseBody).RootElement.GetProperty("message").GetString()! : 
                "Invalid credentials or server error.");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred connecting to the API.");
            return Page();
        }
    }
}
