namespace RaceDayWeb.Models;

public class EventItem
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; }
    public string EventName { get; set; }
    public string Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; }
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; }
    public int EnrolmentCount { get; set; }

    public string BannerUrl { get; set; }
}

public class ImageUploadResult
{
    public string Url { get; set; }
}
public class CategoryItem
{
    public int CategoryID { get; set; }
    public int EventID { get; set; }
    public string CategoryName { get; set; }
    public decimal DistanceKm { get; set; }
    public decimal EntryFee { get; set; }
    public int? MinAge { get; set; }
}

public class EnrolmentItem
{
    public int EnrolmentID { get; set; }
    public int ParticipantID { get; set; }
    public string ParticipantName { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string RaceNumber { get; set; }
    public string Status { get; set; }
    public int? ResultID { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string ResultStatus { get; set; }
}
public class ResultItem
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string ParticipantName { get; set; }
    public string EventName { get; set; }
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; }
    public string RaceNumber { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public string Status { get; set; }
    public int TotalFinishers { get; set; }
}