using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>View the categories of an event and manage them (organisers can own evets only).</summary>
[ApiController]
[Route("api")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Lists all categories for an event.</summary>
    /// <remarks>Role required: Organiser or Participant.</remarks>
    /// <param name="eventId">The event ID.</param>
    /// <response code="200">An array of categories (empty if there are none).</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">No event with that ID.</response>
    [RequireRole]
    [HttpGet("events/{eventId:int}/categories")]
    [ProducesResponseType<List<CategoryResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var categories = await _db.Categories
            .Where(c => c.EventID == eventId)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return Ok(categories.Select(ToResponse));
    }

    /// <summary>Adds a new category to an event the logged-in organiser owns.</summary>
    /// <remarks>Role required: Organiser (owner of the event only). Category names must be unique within an event.</remarks>
    /// <param name="eventId">The event ID.</param>
    /// <param name="dto">Category name, distance, entry fee and optional minimum age.</param>
    /// <response code="201">Category created. Returns the new category.</response>
    /// <response code="400">A field is missing or invalid.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or this is not your event.</response>
    /// <response code="404">No event with that ID.</response>
    /// <response code="409">This event already has a category with that name.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPost("events/{eventId:int}/categories")]
    [ProducesResponseType<CategoryResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int eventId, CategoryDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        if (ev.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only add categories to your own events." });

        var name = dto.CategoryName.Trim();
        if (await _db.Categories.AnyAsync(c => c.EventID == eventId && c.CategoryName == name))
            return Conflict(new { message = "This event already has a category with that name." });

        var category = new Category
        {
            EventID = eventId,
            CategoryName = name,
            DistanceKm = dto.DistanceKm,
            EntryFee = dto.EntryFee,
            MinAge = dto.MinAge
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(201, ToResponse(category));
    }

    /// <summary>Updates a category that belongs to one of the organiser's events.</summary>
    /// <remarks>Role required: Organiser (owner of the event only).</remarks>
    /// <param name="id">The category ID.</param>
    /// <param name="dto">The new category details.</param>
    /// <response code="200">The updated category.</response>
    /// <response code="400">A field is missing or invalid.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or the event is not yours.</response>
    /// <response code="404">No category with that ID.</response>
    /// <response code="409">Another category in this event already has that name.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPut("categories/{id:int}")]
    [ProducesResponseType<CategoryResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (category.Event.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only change categories of your own events." });

        var name = dto.CategoryName.Trim();
        if (await _db.Categories.AnyAsync(c => c.EventID == category.EventID && c.CategoryName == name && c.CategoryID != id))
            return Conflict(new { message = "This event already has a category with that name." });

        category.CategoryName = name;
        category.DistanceKm = dto.DistanceKm;
        category.EntryFee = dto.EntryFee;
        category.MinAge = dto.MinAge;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(category));
    }

    /// <summary>Removes a category from an event the logged-in organiser owns.</summary>
    /// <remarks>
    /// Role required: Organiser (owner of the event only). No request body is needed.
    /// A category that participants have already entered cannot be deleted.
    /// </remarks>
    /// <param name="id">The category ID.</param>
    /// <response code="204">Category deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or the event is not yours.</response>
    /// <response code="404">No category with that ID.</response>
    /// <response code="409">Participants are already enrolled in this category.</response>
    [RequireRole(Roles.Organiser)]
    [HttpDelete("categories/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (category.Event.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only delete categories of your own events." });

        try
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Participants are already enrolled in this category, so it can't be deleted." });
        }

        return NoContent();
    }

    private static CategoryResponseDto ToResponse(Category c)
    {
        return new CategoryResponseDto
        {
            CategoryID = c.CategoryID,
            EventID = c.EventID,
            CategoryName = c.CategoryName,
            DistanceKm = c.DistanceKm,
            EntryFee = c.EntryFee,
            MinAge = c.MinAge
        };
    }
}