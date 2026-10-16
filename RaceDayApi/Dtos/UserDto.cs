using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Dtos;

/// <summary>Profile details a user may change. The role cannot be changed here.</summary>
public class UpdateProfileDto
{
    /// <summary>The user's full name.</summary>
    /// <example> Mahlatse Malatie </example>
    [Required, StringLength(40)]
    public string FullName { get; set; } = "";

    /// <summary>The user's email address. Must not belong to another account.</summary>
    /// <example>Mahlatse@gmail.com</example>
    [Required, EmailAddress, StringLength(90)]
    public string Email { get; set; } = "";
}