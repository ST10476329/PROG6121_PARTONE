namespace RaceDayApi.Helpers
{
    public static class Roles
    {
        public const string Organiser = "Organiser";
        public const string Participant = "Participant";
    }

    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string Role = "Role";
    }

    public static class EventTypes
    {
        public const string Run = "Run";
        public const string Walk = "Walk";
        public const string Cycle = "Cycle";

        public static readonly string[] All = { Run, Walk, Cycle };

        // returns the correctly cased name or returns null if it isn't valid
        public static string? Find(string input)
        {
            return All.FirstOrDefault(t => string.Equals(t, input.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class ResultStatuses
    {
        public const string Finished = "Finished";
        public const string DidNotFinish = "DNF";
        public const string DidNotStart = "DNS";
        public const string Disqualified = "DSQ";

        public static readonly string[] All = { Finished, DidNotFinish, DidNotStart, Disqualified };

        public static string? Find(string input)
        {
            return All.FirstOrDefault(s => string.Equals(s, input.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
