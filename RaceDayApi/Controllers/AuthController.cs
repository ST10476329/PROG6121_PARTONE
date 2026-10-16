using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>Create an account where you can then log in and log out.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(RaceDayDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>Creates a new account as an Organiser or a Participant.</summary>
    /// <remarks>
    /// Role required: none (public).
    /// Organisers must also send the secret organiser code, so not just anyone can become an organiser.
    /// The password is hashed with BCrypt and the plain password is never stored.
    /// </remarks>
    /// <param name="dto">FullName, Email, Password, Role and (organisers only) OrganiserCode.</param>
    /// <response code="201">Account created. Returns the new user (no password).</response>
    /// <response code="400">A field is missing or invalid, or the role is not Organiser or Participant.</response>
    /// <response code="403">The role is Organiser but the organiser code is wrong.</response>
    /// <response code="409">That email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // only the two real roles are allowed
        string role;
        if (string.Equals(dto.Role, Roles.Organiser, StringComparison.OrdinalIgnoreCase))
            role = Roles.Organiser;
        else if (string.Equals(dto.Role, Roles.Participant, StringComparison.OrdinalIgnoreCase))
            role = Roles.Participant;
        else
            return BadRequest(new { message = "Role must be Organiser or Participant." });

        // organisers must send the secret code
        if (role == Roles.Organiser)
        {
            var realCode = _config["OrganiserCode"];

            // if the setting is missing then nobody can register as an organiser
            if (string.IsNullOrWhiteSpace(realCode) || dto.OrganiserCode != realCode)
                return StatusCode(403, new { message = "Invalid organiser code." });
        }

        var email = dto.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "That email is already registered." });

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, ToResponse(user));
    }

    /// <summary>Logs a user in and starts a session.</summary>
    /// <remarks>
    /// Role required: none (public).
    /// The server stores the user ID and role in the session and sends back a session cookie,
    /// so every later request is recognised as this user (the cookie takes the place of a token).
    /// Logging in again as someone else replaces the session, which is handy for switching roles while testing.
    /// </remarks>
    /// <param name="dto">The email and password.</param>
    /// <response code="200">Logged in. Returns the user.</response>
    /// <response code="400">Email or password is missing.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // same message for "no such email" and "wrong password" so we don't leak which one it was
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserID);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role);

        return Ok(new LoginResponseDto { User = ToResponse(user) });
    }

    /// <summary>Logs the user out by ending their session.</summary>
    /// <remarks>Role required: Organiser or Participant.</remarks>
    /// <response code="200">Logged out.</response>
    /// <response code="401">Not logged in.</response>
    [RequireRole]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }

    private static UserResponseDto ToResponse(User user)
    {
        return new UserResponseDto
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role
        };
    }
}