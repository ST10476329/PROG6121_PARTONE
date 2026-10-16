using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>View events (everyone) and manage them (organisers, own events only).</summary>
[ApiController]
[Route("api/events")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Lists all events, soonest first.</summary>
    /// <remarks>Role required: Organiser or Participant.</remarks>
    /// <response code="200">An array of all events (empty if there are none).</response>
    /// <response code="401">Not logged in.</response>
    [RequireRole]
    [HttpGet]
    [ProducesResponseType<List<EventResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .Include(e => e.Organiser)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return Ok(events.Select(ToResponse));
    }

    /// <summary>Returns the details of one event.</summary>
    /// <remarks>Role required: Organiser or Participant.</remarks>
    /// <param name="id">The event ID.</param>
    /// <response code="200">The event.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">No event with that ID.</response>
    [RequireRole]
    [HttpGet("{id:int}")]
    [ProducesResponseType<EventResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        return Ok(ToResponse(ev));
    }

    /// <summary>Creates a new event owned by the logged-in organiser.</summary>
    /// <remarks>
    /// Role required: Organiser.
    /// The event type must be Run, Walk or Cycle, and the date cannot be in the past.
    /// </remarks>
    /// <param name="dto">Name, description, date, location, distance and event type.</param>
    /// <response code="201">Event created. Returns the new event.</response>
    /// <response code="400">A field is invalid, the event type is not Run/Walk/Cycle, or the date is in the past.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only organisers can create events.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPost]
    [ProducesResponseType<EventResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(EventDto dto)
    {
        var type = EventTypes.Find(dto.EventType);
        if (type == null)
            return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });

        if (dto.EventDate!.Value.Date < DateTime.Today)
            return BadRequest(new { message = "The event date cannot be in the past." });

        var ev = new Event
        {
            OrganiserID = HttpContext.Session.GetUserId(),   // the logged-in organiser owns it
            EventName = dto.EventName.Trim(),
            Description = dto.Description.Trim(),
            EventDate = dto.EventDate.Value.Date,
            Location = dto.Location.Trim(),
            DistanceKm = dto.DistanceKm,
            EventType = type
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();   // so the response has the name
        return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, ToResponse(ev));
    }

    /// <summary>Updates an event the logged-in organiser owns.</summary>
    /// <remarks>Role required: Organiser (owner of the event only).</remarks>
    /// <param name="id">The event ID.</param>
    /// <param name="dto">The new event details.</param>
    /// <response code="200">The updated event.</response>
    /// <response code="400">A field is invalid or the event type is not Run/Walk/Cycle.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or this is not your event.</response>
    /// <response code="404">No event with that ID.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPut("{id:int}")]
    [ProducesResponseType<EventResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, EventDto dto)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventID == id);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        if (ev.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only change your own events." });

        var type = EventTypes.Find(dto.EventType);
        if (type == null)
            return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });

        ev.EventName = dto.EventName.Trim();
        ev.Description = dto.Description.Trim();
        ev.EventDate = dto.EventDate!.Value.Date;
        ev.Location = dto.Location.Trim();
        ev.DistanceKm = dto.DistanceKm;
        ev.EventType = type;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(ev));
    }

    /// <summary>Deletes an event the logged-in organiser owns.</summary>
    /// <remarks>
    /// Role required: Organiser (owner of the event only). No request body is needed.
    /// An event that still has categories or other data linked to it cannot be deleted.
    /// </remarks>
    /// <param name="id">The event ID.</param>
    /// <response code="204">Event deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or this is not your event.</response>
    /// <response code="404">No event with that ID.</response>
    /// <response code="409">The event still has linked data (e.g. categories).</response>
    [RequireRole(Roles.Organiser)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        if (ev.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only delete your own events." });

        try
        {
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // no deletes so the database refuses if anything still points at the event
            return Conflict(new { message = "This event still has categories or other data linked to it. Remove those first." });
        }

        return NoContent();
    }

    private static EventResponseDto ToResponse(Event e)
    {
        return new EventResponseDto
        {
            EventID = e.EventID,
            OrganiserID = e.OrganiserID,
            OrganiserName = e.Organiser.FullName,
            EventName = e.EventName,
            Description = e.Description,
            EventDate = e.EventDate,
            Location = e.Location,
            DistanceKm = e.DistanceKm,
            EventType = e.EventType
        };
    }
}