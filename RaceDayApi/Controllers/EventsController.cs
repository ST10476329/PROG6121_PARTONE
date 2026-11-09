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

    /// <summary>Lists the events owned by the logged-in organiser, soonest first.</summary>
    /// <remarks>Role required: Organiser. Each event includes how many people are enrolled in it.</remarks>
    /// <response code="200">An array of the organiser's own events (empty if there are none).</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only organisers have events to manage.</response>
    [RequireRole(Roles.Organiser)]
    [HttpGet("mine")]
    [ProducesResponseType<List<EventResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        int organiserId = HttpContext.Session.GetUserId();

        List<Event> events = await _db.Events
            .Include(e => e.Organiser)
            .Where(e => e.OrganiserID == organiserId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return Ok(await ToResponseList(events));
    }

    // how many people are enrolled in each event (EventID -> number of enrolments)
    private async Task<Dictionary<int, int>> GetEnrolmentCounts()
    {
        var rows = await _db.Registrations
            .GroupBy(r => r.Category.EventID)
            .Select(g => new { EventID = g.Key, Total = g.Count() })
            .ToListAsync();

        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var row in rows)
        {
            counts.Add(row.EventID, row.Total);
        }
        return counts;
    }

    // turns events into response objects that include their enrolment counts
    private async Task<List<EventResponseDto>> ToResponseList(List<Event> events)
    {
        Dictionary<int, int> counts = await GetEnrolmentCounts();

        List<EventResponseDto> list = new List<EventResponseDto>();
        foreach (Event e in events)
        {
            EventResponseDto dto = ToResponse(e);
            if (counts.ContainsKey(e.EventID))
            {
                dto.EnrolmentCount = counts[e.EventID];
            }
            list.Add(dto);
        }
        return list;
    }
    /// <summary>Lists events, soonest first. Can be filtered.</summary>
    /// <remarks>Role required: Organiser or Participant. All filters are optional and can be combined.</remarks>
    /// <param name="type">Only events of this type: Run, Walk or Cycle.</param>
    /// <param name="search">Only events whose name or location contains this text.</param>
    /// <param name="upcomingOnly">If true, only events happening today or later.</param>
    /// <response code="200">An array of matching events (empty if none match).</response>
    /// <response code="400">The type is not Run, Walk or Cycle.</response>
    /// <response code="401">Not logged in.</response>
    [RequireRole]
    [HttpGet]
    [ProducesResponseType<List<EventResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(string? type = null, string? search = null, bool upcomingOnly = false)
    {
        IQueryable<Event> query = _db.Events.Include(e => e.Organiser);

        if (!string.IsNullOrWhiteSpace(type))
        {
            string? cleanType = EventTypes.Find(type);
            if (cleanType == null)
                return BadRequest(new { message = "Type must be Run, Walk or Cycle." });

            query = query.Where(e => e.EventType == cleanType);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.EventName.Contains(search) || e.Location.Contains(search));
        }

        if (upcomingOnly)
        {
            query = query.Where(e => e.EventDate >= DateTime.Today);
        }

        List<Event> events = await query.OrderBy(e => e.EventDate).ToListAsync();
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
            EventType = e.EventType,
            BannerUrl = e.BannerUrl,
        };
    }
}