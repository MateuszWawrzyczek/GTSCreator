public static class TimeUtils
{
    public static List<string> GetNextNDays(int n)
    {
        var days = new List<string>();
        for (int i = 0; i < n; i++)
        {
            days.Add(DateTime.UtcNow.AddDays(i).ToString("yyyy-MM-dd"));
        }
        return days;
    }

    public static int TimeToSeconds(string timeStr)
    {
        var parts = timeStr.Split(':').Select(int.Parse).ToArray();
        return parts[0] * 3600 + parts[1] * 60;
    }

    public static string SecondsToTime(int seconds)
    {
        int h = seconds / 3600;
        int m = (seconds % 3600) / 60;
        int s = seconds % 60;
        return $"{h:D2}:{m:D2}:{s:D2}";
    }
}
