using System.ComponentModel.DataAnnotations;

namespace RaceDayWeb.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(40)]
    [Display(Name = "Full name")]
    public string FullName { get; set; }

    [Required, EmailAddress, StringLength(90)]
    public string Email { get; set; }

    [Required, StringLength(72, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; }

    [Required]
    public string Role { get; set; }

    [Display(Name = "Organiser code")]
    public string OrganiserCode { get; set; }
}

// what the API's login endpoint sends back
public class ApiUser
{
    public int UserID { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public string ProfilePictureUrl { get; set; }
}

public class LoginApiResult
{
    public ApiUser User { get; set; }
}