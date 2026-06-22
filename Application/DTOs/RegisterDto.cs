using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserName {  get; set; }
        [EmailAddress]
        [Required]
        public string Email {  get; set; }
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+{}|:""<>?,./;'\[\]\\`~\-=]).{6,}$",ErrorMessage = "Password Must have 1 Uppercase, 1 Lowercase, 1 number, 1 non alphanumeric and at least 6 characters")]
        [Required]
        public string Password { get; set; }
        [Phone]
        [RegularExpression(@"^\+?[0-9]{1,15}$", ErrorMessage = "Phone number must be numeric and must not exceed 15 digits")]
        public string? PhoneNumber {  get; set; }

    }
}
