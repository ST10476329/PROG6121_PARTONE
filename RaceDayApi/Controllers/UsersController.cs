using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>View and update your own profile.</summary>
[ApiController]
[Route("api/users")]
[RequireRole]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns the logged-in user's profile.</summary>
    /// <remarks>Role required: Organiser or Participant. The user comes from the session, so no ID is needed.</remarks>
    /// <response code="200">The user's profile.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("me")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe()
    {
        var user = await _db.Users.FindAsync(HttpContext.Session.GetUserId());
        if (user == null)
        {
            HttpContext.Session.Clear();   // the account was deleted, so end the session
            return Unauthorized(new { message = "Please log in first." });
        }

        return Ok(ToResponse(user));
    }

    /// <summary>Updates the logged-in user's name and email.</summary>
    /// <remarks>
    /// Role required: Organiser or Participant.
    /// The role is not part of the request, so nobody can promote themselves.
    /// </remarks>
    /// <param name="dto">The new full name and email.</param>
    /// <response code="200">The updated profile.</response>
    /// <response code="400">A field is missing or invalid.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="409">The email already belongs to another account.</response>
    [HttpPut("me")]
    [ProducesResponseType<UserResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
    {
        var userId = HttpContext.Session.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            HttpContext.Session.Clear();
            return Unauthorized(new { message = "Please log in first." });
        }

        var email = dto.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserID != userId))
            return Conflict(new { message = "That email is already used by another account." });

        user.FullName = dto.FullName.Trim();
        user.Email = email;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(user));
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