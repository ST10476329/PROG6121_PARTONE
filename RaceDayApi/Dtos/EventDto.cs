using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Dtos;

/// <summary>Details of an event, used to create or update one.</summary>
public class EventDto
{
    /// <summary>Name of the event.</summary>
    /// <example>Joburg City Run</example>
    [Required, StringLength(200)]
    public string EventName { get; set; } = "";

    /// <summary>What the event is about.</summary>
    /// <example>A fun 10km run through the city.</example>
    [Required, StringLength(250)]
    public string Description { get; set; } = "";

    /// <summary>Date of the event (yyyy-MM-dd).</summary>
    /// <example>2026-12-05</example>
    [Required]
    public DateTime? EventDate { get; set; }

    /// <summary>Where the event takes place.</summary>
    /// <example>Johannesburg</example>
    [Required, StringLength(50)]
    public string Location { get; set; } = "";

    /// <summary>Distance in kilometres.</summary>
    /// <example>10</example>
    [Range(0.1, 999.99)]
    public decimal DistanceKm { get; set; }

    /// <summary>Run, Walk or Cycle.</summary>
    /// <example>Run</example>
    [Required]
    public string EventType { get; set; } = "";
}

public class EventResponseDto
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = "";
    public string EventName { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
}