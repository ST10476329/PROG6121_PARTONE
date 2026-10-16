namespace RaceDayApi.Helpers
{
    public static class SessionHelper
    {
        // his will only be used on endpoints with [RequireRole], so someone is always logged in
        public static int GetUserId(this ISession session)
        {
            return session.GetInt32(SessionKeys.UserId) ?? 0;
        }
    }
}
