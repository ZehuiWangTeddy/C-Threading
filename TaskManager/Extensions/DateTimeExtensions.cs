namespace TaskManager.Extensions;

public static class DateTimeExtensions
{
    public static DateTime RoundToMinutes(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
    }

    public static TimeSpan RoundToMinutes(this TimeSpan time)
    {
        return new TimeSpan(time.Hours, time.Minutes, 0);
    }
}