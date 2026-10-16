using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>Participants enter events by choosing a category. Organisers view who has entered.</summary>
[ApiController]
[Route("api")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Enrols the logged-in participant in an event under a chosen category.</summary>
    /// <remarks>
    /// Role required: Participant.
    /// Records the link between the participant, the event and the category, and generates a unique race number.
    /// A participant can enter an event only once, and cannot enter an event that has already taken place.
    /// </remarks>
    /// <param name="eventId">The event to enter.</param>
    /// <param name="dto">The ID of the chosen category.</param>
    /// <response code="201">Enrolled. Returns the enrolment with the race number.</response>
    /// <response code="400">The category does not belong to this event, or the event has already taken place.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only participants can enrol.</response>
    /// <response code="404">The event or the category does not exist.</response>
    /// <response code="409">You are already enrolled in this event.</response>
    [RequireRole(Roles.Participant)]
    [HttpPost("events/{eventId:int}/enrolments")]
    [ProducesResponseType<EnrolmentResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int eventId, EnrolmentDto dto)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        if (ev.EventDate.Date < DateTime.Today)
            return BadRequest(new { message = "This event has already taken place." });

        // the category must exist (404) and belong to THIS event (400)
        var category = await _db.Categories.FindAsync(dto.CategoryID);
        if (category == null)
            return NotFound(new { message = "Category not found." });

        if (category.EventID != eventId)
            return BadRequest(new { message = "That category does not belong to this event." });

        // one entry per event, whichever category they chose
        var userId = HttpContext.Session.GetUserId();
        var alreadyIn = await _db.Registrations
            .AnyAsync(r => r.ParticipantID == userId && r.Category.EventID == eventId);
        if (alreadyIn)
            return Conflict(new { message = "You are already enrolled in this event." });

        // make a race number nobody else has (the column is UNIQUE)
        string raceNumber;
        do
        {
            raceNumber = "RD" + Random.Shared.Next(10000, 99999);
        }
        while (await _db.Registrations.AnyAsync(r => r.RaceNumber == raceNumber));

        var registration = new Registration
        {
            ParticipantID = userId,
            CategoryID = category.CategoryID,
            RaceNumber = raceNumber
            // RegistrationDate is filled in by the database (DEFAULT GETDATE())
        };

        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync();

        var saved = await EnrolmentsWithDetails().FirstAsync(r => r.RegistrationID == registration.RegistrationID);
        return StatusCode(201, ToResponse(saved));
    }

    /// <summary>Returns all of the logged-in participant's enrolments across all events.</summary>
    /// <remarks>Role required: Participant. Soonest event first.</remarks>
    /// <response code="200">An array of enrolments (empty if none).</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only participants have enrolments.</response>
    [RequireRole(Roles.Participant)]
    [HttpGet("users/me/enrolments")]
    [ProducesResponseType<List<EnrolmentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId();
        var enrolments = await EnrolmentsWithDetails()
            .Where(r => r.ParticipantID == userId)
            .OrderBy(r => r.Category.Event.EventDate)
            .ToListAsync();

        return Ok(enrolments.Select(ToResponse));
    }

    /// <summary>Returns everyone enrolled in an event the logged-in organiser owns.</summary>
    /// <remarks>Role required: Organiser (owner of the event only).</remarks>
    /// <param name="eventId">The event ID.</param>
    /// <response code="200">An array of enrolments (empty if none).</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or this is not your event.</response>
    /// <response code="404">No event with that ID.</response>
    [RequireRole(Roles.Organiser)]
    [HttpGet("events/{eventId:int}/enrolments")]
    [ProducesResponseType<List<EnrolmentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null)
            return NotFound(new { message = "Event not found." });

        if (ev.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only view enrolments for your own events." });

        var enrolments = await EnrolmentsWithDetails()
            .Where(r => r.Category.EventID == eventId)
            .OrderBy(r => r.RaceNumber)
            .ToListAsync();

        return Ok(enrolments.Select(ToResponse));
    }

    /// <summary>Cancels the logged-in participant's own enrolment.</summary>
    /// <remarks>
    /// Role required: Participant (owner of the enrolment only). No request body is needed.
    /// An enrolment that already has a result or payment cannot be cancelled.
    /// </remarks>
    /// <param name="id">The enrolment ID.</param>
    /// <response code="204">Enrolment cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not a participant, or this is not your enrolment.</response>
    /// <response code="404">No enrolment with that ID.</response>
    /// <response code="409">The enrolment already has a result or payment.</response>
    [RequireRole(Roles.Participant)]
    [HttpDelete("enrolments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        var registration = await _db.Registrations.FindAsync(id);
        if (registration == null)
            return NotFound(new { message = "Enrolment not found." });

        if (registration.ParticipantID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only cancel your own enrolment." });

        try
        {
            _db.Registrations.Remove(registration);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "This enrolment already has a result or payment, so it can't be cancelled." });
        }

        return NoContent();
    }

    // loads an enrolment together with the participant, category and event
    private IQueryable<Registration> EnrolmentsWithDetails()
    {
        return _db.Registrations
            .Include(r => r.Participant)
            .Include(r => r.Category).ThenInclude(c => c.Event);
    }

    private static EnrolmentResponseDto ToResponse(Registration r)
    {
        return new EnrolmentResponseDto
        {
            EnrolmentID = r.RegistrationID,
            ParticipantID = r.ParticipantID,
            ParticipantName = r.Participant.FullName,
            EventID = r.Category.EventID,
            EventName = r.Category.Event.EventName,
            CategoryID = r.CategoryID,
            CategoryName = r.Category.CategoryName,
            RegistrationDate = r.RegistrationDate,
            RaceNumber = r.RaceNumber
        };
    }
}