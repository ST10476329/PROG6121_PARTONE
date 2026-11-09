namespace RaceDayWeb.Helpers;

public static class Roles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}

// the names we save things under in the MVC session
public static class SessionKeys
{
    public const string ApiCookie = "ApiCookie";
    public const string UserId = "UserId";
    public const string Role = "Role";
    public const string FullName = "FullName";
    public const string Picture = "Picture";
}