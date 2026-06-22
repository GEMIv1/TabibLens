using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TabibLens.Web.Services;

namespace TabibLens.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly ApiService _apiService;

    public RegisterModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";

        [Phone]
        [RegularExpression(@"^\+?[0-9]{1,15}$", ErrorMessage = "Phone number must be numeric and not exceed 15 digits")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+{}|:""<>?,./;'\[\]\\`~\-=]).{6,}$", 
            ErrorMessage = "Password must have 1 Uppercase, 1 Lowercase, 1 number, 1 non-alphanumeric and at least 6 characters")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = "";
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
            var request = new RegisterRequest
            {
                UserName = Input.UserName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                Password = Input.Password
            };

            var response = await _apiService.RegisterAsync(request);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                HttpContext.Session.SetString("AccessToken", response.AccessToken);
                HttpContext.Session.SetString("UserName", response.UserName);
                HttpContext.Session.SetString("UserEmail", response.Email);

                TempData["SuccessMessage"] = "Account created successfully!";
                return RedirectToPage("/Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Registration failed.");
            return Page();
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.ResponseBody.Contains("message") ? 
                System.Text.Json.JsonDocument.Parse(ex.ResponseBody).RootElement.GetProperty("message").GetString()! : 
                "Registration failed. Email may already be in use.");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred connecting to the API.");
            return Page();
        }
    }
}
