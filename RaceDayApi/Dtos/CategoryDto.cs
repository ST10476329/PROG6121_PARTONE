using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Dtos;

/// <summary>Details of a category, used to create or update one.</summary>
public class CategoryDto
{
    /// <summary>Name of the category, e.g. Under 20, Senior, 10km.</summary>
    /// <example>10km</example>
    [Required, StringLength(50)]
    public string CategoryName { get; set; } = "";

    /// <summary>Distance for this category in kilometres.</summary>
    /// <example>10</example>
    [Range(0.1, 999.99)]
    public decimal DistanceKm { get; set; }

    /// <summary>Entry fee in rand. Use 0 for a free category.</summary>
    /// <example>150</example>
    [Range(0, 999999.99)]
    public decimal EntryFee { get; set; }

    /// <summary>Minimum age to enter (optional).</summary>
    /// <example>16</example>
    [Range(0, 120)]
    public int? MinAge { get; set; }
}

public class CategoryResponseDto
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int? MinAge { get; set; }
}