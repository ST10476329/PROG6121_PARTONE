using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Dtos;

/// <summary>The category a participant wants to enter.</summary>
public class EnrolmentDto
{
    /// <summary>ID of the chosen category. It must belong to the event in the URL.</summary>
    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int CategoryID { get; set; }
}

public class EnrolmentResponseDto
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; } = "";
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime RegistrationDate { get; set; }
    public string RaceNumber { get; set; } = "";

    /// <summary>Confirmed (event still to come), Awaiting results (event over) or Completed (result captured).</summary>
    public string Status { get; set; } = "";

    // filled in once the organiser has captured a result
    public int? ResultID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string? ResultStatus { get; set; }
}