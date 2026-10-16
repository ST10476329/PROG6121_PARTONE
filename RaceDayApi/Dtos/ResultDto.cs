using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Dtos;

/// <summary>A participant's result in an event.</summary>
public class ResultDto
{
    /// <summary>Finish time as hh:mm:ss. Required when the status is Finished.</summary>
    /// <example>01:45:30</example>
    public TimeSpan? FinishTime { get; set; }

    /// <summary>Finishing position in the category. Required when the status is Finished.</summary>
    /// <example>1</example>
    [Range(1, 100000)]
    public int? Position { get; set; }

    /// <summary>Finished, DNF (did not finish), DNS (did not start) or DSQ (disqualified).</summary>
    /// <example>Finished</example>
    [Required]
    public string Status { get; set; } = "";
}

public class ResultResponseDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string ParticipantName { get; set; } = "";
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = "";
    public string RaceNumber { get; set; } = "";
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; } = "";
}