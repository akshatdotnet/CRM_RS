using System.ComponentModel.DataAnnotations;

namespace HRMS.WebPortal.Models.Auth;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }
}
