using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.BlazorUI.Models.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = $"Password must be at least 6 characters")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).*$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter and one digit")]
    public string Password { get; set; } = string.Empty;
}