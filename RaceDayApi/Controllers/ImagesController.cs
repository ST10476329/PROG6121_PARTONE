using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>Upload event banners and profile pictures. Images are saved in the API's wwwroot folder.</summary>
[ApiController]
[Route("api")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ImagesController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ImagesController(RaceDayDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    /// <summary>Uploads a banner image for an event the logged-in organiser owns.</summary>
    /// <remarks>
    /// Role required: Organiser (owner of the event only).
    /// Send the image as multipart/form-data in a field called "file". JPG, PNG or WebP, up to 2 MB.
    /// The image is saved in wwwroot and its URL is saved in the database. A new upload replaces the old banner.
    /// </remarks>
    /// <param name="id">The event ID.</param>
    /// <param name="file">The image file.</param>
    /// <response code="200">Banner saved. Returns the image URL.</response>
    /// <response code="400">No file, wrong file type, or the file is bigger than 2 MB.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or this is not your event.</response>
    /// <response code="404">No event with that ID.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPost("events/{id:int}/banner")]
    [ProducesResponseType<ImageUploadResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        Event? ev = await _db.Events.FindAsync(id);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        if (ev.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only change the banner of your own events." });

        string? error = ImageHelper.CheckImage(file);
        if (error != null)
            return BadRequest(new { message = error });

        string newUrl = await ImageHelper.SaveImage(file, "banners", _env.WebRootPath);
        string? oldUrl = ev.BannerUrl;

        ev.BannerUrl = newUrl;
        await _db.SaveChangesAsync();

        ImageHelper.DeleteImage(oldUrl, _env.WebRootPath);   // remove the old file

        return Ok(new ImageUploadResponseDto { Url = newUrl });
    }

    /// <summary>Uploads a profile picture for the logged-in user.</summary>
    /// <remarks>
    /// Role required: Organiser or Participant.
    /// Send the image as multipart/form-data in a field called "file". JPG, PNG or WebP, up to 2 MB.
    /// The image is saved in wwwroot and its URL is saved in the database. A new upload replaces the old picture.
    /// </remarks>
    /// <param name="file">The image file.</param>
    /// <response code="200">Picture saved. Returns the image URL.</response>
    /// <response code="400">No file, wrong file type, or the file is bigger than 2 MB.</response>
    /// <response code="401">Not logged in.</response>
    [RequireRole]
    [HttpPost("users/me/picture")]
    [ProducesResponseType<ImageUploadResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        User? user = await _db.Users.FindAsync(HttpContext.Session.GetUserId());
        if (user == null)
        {
            HttpContext.Session.Clear();
            return Unauthorized(new { message = "Please log in first." });
        }

        string? error = ImageHelper.CheckImage(file);
        if (error != null)
            return BadRequest(new { message = error });

        string newUrl = await ImageHelper.SaveImage(file, "profiles", _env.WebRootPath);
        string? oldUrl = user.ProfilePictureUrl;

        user.ProfilePictureUrl = newUrl;
        await _db.SaveChangesAsync();

        ImageHelper.DeleteImage(oldUrl, _env.WebRootPath);

        return Ok(new ImageUploadResponseDto { Url = newUrl });
    }
}