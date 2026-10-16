using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers;

/// <summary>Organisers capture results. Participants view their own.</summary>
[ApiController]
[Route("api")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Captures a finish time, position and status for a participant's enrolment.</summary>
    /// <remarks>
    /// Role required: Organiser (owner of the event only).
    /// A Finished result needs a finish time and a position. Two people in the same category cannot share a position.
    /// An enrolment can only have one result. Use PUT to change it.
    /// </remarks>
    /// <param name="enrolmentId">The enrolment the result belongs to.</param>
    /// <param name="dto">Finish time (hh:mm:ss), position and status.</param>
    /// <response code="201">Result created. Returns the new result.</response>
    /// <response code="400">A field is invalid, a Finished result is missing a time or position, or the position is already taken.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or the event is not yours.</response>
    /// <response code="404">No enrolment with that ID.</response>
    /// <response code="409">This enrolment already has a result.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPost("enrolments/{enrolmentId:int}/results")]
    [ProducesResponseType<ResultResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int enrolmentId, ResultDto dto)
    {
        var registration = await _db.Registrations
            .Include(r => r.Category).ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(r => r.RegistrationID == enrolmentId);
        if (registration == null)
            return NotFound(new { message = "Enrolment not found." });

        if (registration.Category.Event.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only capture results for your own events." });

        if (await _db.Results.AnyAsync(r => r.RegistrationID == enrolmentId))
            return Conflict(new { message = "This enrolment already has a result. Use PUT to change it." });

        var error = await CheckResult(dto, registration.CategoryID, 0);
        if (error != null)
            return BadRequest(new { message = error });

        var result = new Result
        {
            RegistrationID = enrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            Status = ResultStatuses.Find(dto.Status)!
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var saved = await ResultsWithDetails().FirstAsync(r => r.ResultID == result.ResultID);
        return StatusCode(201, ToResponse(saved));
    }

    /// <summary>Updates an existing result.</summary>
    /// <remarks>Role required: Organiser (owner of the event only). The same rules as creating a result apply.</remarks>
    /// <param name="id">The result ID.</param>
    /// <param name="dto">The corrected finish time, position and status.</param>
    /// <response code="200">The updated result.</response>
    /// <response code="400">A field is invalid, a Finished result is missing a time or position, or the position is already taken.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">You are not an organiser, or the event is not yours.</response>
    /// <response code="404">No result with that ID.</response>
    [RequireRole(Roles.Organiser)]
    [HttpPut("results/{id:int}")]
    [ProducesResponseType<ResultResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ResultDto dto)
    {
        var result = await ResultsWithDetails().FirstOrDefaultAsync(r => r.ResultID == id);
        if (result == null)
            return NotFound(new { message = "Result not found." });

        if (result.Registration.Category.Event.OrganiserID != HttpContext.Session.GetUserId())
            return StatusCode(403, new { message = "You can only change results for your own events." });

        var error = await CheckResult(dto, result.Registration.CategoryID, id);
        if (error != null)
            return BadRequest(new { message = error });

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.Status = ResultStatuses.Find(dto.Status)!;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(result));
    }

    /// <summary>Returns the logged-in participant's full performance history.</summary>
    /// <remarks>Role required: Participant. Only your own results are returned, newest event first, with the event and category.</remarks>
    /// <response code="200">An array of results (empty if none).</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only participants have a performance history.</response>
    [RequireRole(Roles.Participant)]
    [HttpGet("users/me/results")]
    [ProducesResponseType<List<ResultResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var userId = HttpContext.Session.GetUserId();
        var results = await ResultsWithDetails()
            .Where(r => r.Registration.ParticipantID == userId)
            .OrderByDescending(r => r.Registration.Category.Event.EventDate)
            .ToListAsync();

        return Ok(results.Select(ToResponse));
    }

    // returns an error message, or null if the result is fine
    private async Task<string?> CheckResult(ResultDto dto, int categoryId, int thisResultId)
    {
        var status = ResultStatuses.Find(dto.Status);
        if (status == null)
            return "Status must be Finished, DNF, DNS or DSQ.";

        if (status == ResultStatuses.Finished && (dto.FinishTime == null || dto.Position == null))
            return "A finished result needs a finish time and a position.";

        if (dto.FinishTime != null && (dto.FinishTime <= TimeSpan.Zero || dto.FinishTime >= TimeSpan.FromHours(24)))
            return "Finish time must be between 00:00:01 and 23:59:59.";

        // two people in the same category can't both come 1st
        if (dto.Position != null)
        {
            var taken = await _db.Results.AnyAsync(r =>
                r.Registration.CategoryID == categoryId &&
                r.Position == dto.Position &&
                r.ResultID != thisResultId);
            if (taken)
                return "Another participant in this category already has that position.";
        }

        return null;
    }

    private IQueryable<Result> ResultsWithDetails()
    {
        return _db.Results
            .Include(r => r.Registration).ThenInclude(g => g.Participant)
            .Include(r => r.Registration).ThenInclude(g => g.Category).ThenInclude(c => c.Event);
    }

    private static ResultResponseDto ToResponse(Result r)
    {
        return new ResultResponseDto
        {
            ResultID = r.ResultID,
            EnrolmentID = r.RegistrationID,
            ParticipantName = r.Registration.Participant.FullName,
            EventName = r.Registration.Category.Event.EventName,
            EventDate = r.Registration.Category.Event.EventDate,
            CategoryName = r.Registration.Category.CategoryName,
            RaceNumber = r.Registration.RaceNumber,
            FinishTime = r.FinishTime,
            Position = r.Position,
            Status = r.Status
        };
    }
}