using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Dtos
{
    /// <summary>
    /// the details needed to create an account
    /// </summary>
    public class RegisterDto
    {
        /// <summary> the users full name. </summary>
        /// <exmaple>Xaba Luyanda</exmaple>
        [Required, StringLength(40)]
        public string FullName { get; set; } = "";

        /// <summary>Email address used to log in. Must always be unique </summary>
        [Required, EmailAddress, StringLength(90)]
        public string Email { get; set; } = "";

        /// <summary>Password 8 to 72 characters, it is hashed before its stored </summary>
        /// /// <exmaple> Password600 </exmaple>
        [Required, StringLength(72, MinimumLength = 8)]
        public string Password { get; set; } = "";
        
        /// <summary> Must either be an organiser or an participant</summary>
        /// <exmaple>Participant</exmaple>
        [Required]
        public string Role { get; set; } = "";

        public string? OrganiserCode { get; set; } // only needed when Role = Organiser
    }
    /// <summary> Credentials for logging in </summary>
    public class LoginDto
    {
        /// <summary> The email address the account was registred with </summary>
        /// <example> xaba@test.com </example>
        [Required]
        public string Email { get; set; } = "";

        /// <summary> The account password </summary>
        /// <example> Password600</example>
        [Required]
        public string Password { get; set; } = "";
    }

    /// <summary> The users details but the password is never included </summary>
    public class UserResponseDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }

    /// <summary>What login returns. The session cookie takes the place of a token.</summary>
    public class LoginResponseDto
    {
        public UserResponseDto User { get; set; } = new UserResponseDto();
    }
}
