using System.ComponentModel.DataAnnotations;

namespace RaceDayWeb.Models;

public class EventDetailsViewModel
{
    public EventItem Event { get; set; }
    public List<CategoryItem> Categories { get; set; }
    public bool IsParticipant { get; set; }
    public EnrolmentItem MyEnrolment { get; set; }   // null if the participant is not enrolled
}

public class ProfileViewModel
{
    [Required, StringLength(40)]
    [Display(Name = "Full name")]
    public string FullName { get; set; }

    [Required, EmailAddress, StringLength(90)]
    public string Email { get; set; }

    public string ProfilePictureUrl { get; set; }
}

public class ParticipantHomeViewModel
{
    public List<EventItem> Events { get; set; }
    public List<int> EnrolledEventIds { get; set; }   // the events this participant has already entered
    public string Search { get; set; }
    public string Type { get; set; }
    public bool ShowPast { get; set; }
}