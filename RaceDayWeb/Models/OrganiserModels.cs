using System.ComponentModel.DataAnnotations;

namespace RaceDayWeb.Models;

public class OrganiserDashboardViewModel
{
    public List<EventItem> Events { get; set; }
    public int TotalEvents { get; set; }
    public int TotalEnrolments { get; set; }
    public int UpcomingEvents { get; set; }
    public DateTime? NextEventDate { get; set; }
}

public class EventFormViewModel
{
    public int EventID { get; set; }

    [Required, StringLength(200)]
    [Display(Name = "Event name")]
    public string EventName { get; set; }

    [Required, StringLength(250)]
    public string Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Event date")]
    public DateTime EventDate { get; set; }

    [Required, StringLength(50)]
    public string Location { get; set; }

    [Range(0.1, 999.99, ErrorMessage = "The distance must be between 0.1 and 999.99 km.")]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Required]
    [Display(Name = "Event type")]
    public string EventType { get; set; }

    [Display(Name = "Banner image")]
    public IFormFile BannerFile { get; set; }
    public string BannerUrl { get; set; }
}

public class CategoryFormViewModel
{
    public int EventID { get; set; }
    public int CategoryID { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; }

    [Range(0.1, 999.99, ErrorMessage = "The distance must be between 0.1 and 999.99 km.")]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Enter a valid entry fee.")]
    [Display(Name = "Entry fee (R)")]
    public decimal EntryFee { get; set; }

    [Range(0, 120, ErrorMessage = "Enter an age between 0 and 120.")]
    [Display(Name = "Minimum age (optional)")]
    public int? MinAge { get; set; }
}

public class CategoryListViewModel
{
    public EventItem Event { get; set; }
    public List<CategoryItem> Categories { get; set; }
}

// used by both the enrolments page and the results page
public class EnrolmentListViewModel
{
    public EventItem Event { get; set; }
    public List<EnrolmentItem> Enrolments { get; set; }
}

public class ResultFormViewModel
{
    public int EventID { get; set; }
    public int EnrolmentID { get; set; }
    public int? ResultID { get; set; }
    public string FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; }
}