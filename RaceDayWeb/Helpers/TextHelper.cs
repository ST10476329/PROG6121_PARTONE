namespace RaceDayWeb.Helpers;

public static class TextHelper
{
    // 1 -> 1st, 2 -> 2nd, 3 -> 3rd, 4 -> 4th, 11 -> 11th, 21 -> 21st
    public static string Ordinal(int number)
    {
        int lastTwo = number % 100;
        if (lastTwo >= 11 && lastTwo <= 13)
            return number + "th";

        int last = number % 10;
        if (last == 1)
            return number + "st";
        if (last == 2)
            return number + "nd";
        if (last == 3)
            return number + "rd";

        return number + "th";
    }
}