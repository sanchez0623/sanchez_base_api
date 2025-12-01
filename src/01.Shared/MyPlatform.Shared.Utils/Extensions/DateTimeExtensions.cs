namespace MyPlatform.Shared.Utils.Extensions;

/// <summary>
/// Extension methods for DateTime operations.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>The Unix timestamp.</returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts a DateTime to Unix timestamp (milliseconds since epoch).
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>The Unix timestamp in milliseconds.</returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Creates a DateTime from a Unix timestamp (seconds).
    /// </summary>
    /// <param name="timestamp">The Unix timestamp in seconds.</param>
    /// <returns>The DateTime.</returns>
    public static DateTime FromUnixTimeSeconds(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    /// <summary>
    /// Creates a DateTime from a Unix timestamp (milliseconds).
    /// </summary>
    /// <param name="timestamp">The Unix timestamp in milliseconds.</param>
    /// <returns>The DateTime.</returns>
    public static DateTime FromUnixTimeMilliseconds(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
    }

    /// <summary>
    /// Gets the start of the day.
    /// </summary>
    /// <param name="dateTime">The DateTime.</param>
    /// <returns>The start of the day.</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day.
    /// </summary>
    /// <param name="dateTime">The DateTime.</param>
    /// <returns>The end of the day.</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday).
    /// </summary>
    /// <param name="dateTime">The DateTime.</param>
    /// <returns>The start of the week.</returns>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dateTime.AddDays(-diff).Date;
    }

    /// <summary>
    /// Gets the start of the month.
    /// </summary>
    /// <param name="dateTime">The DateTime.</param>
    /// <returns>The start of the month.</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Gets the end of the month.
    /// </summary>
    /// <param name="dateTime">The DateTime.</param>
    /// <returns>The end of the month.</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Checks if a DateTime is between two other DateTimes.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <param name="start">The start DateTime.</param>
    /// <param name="end">The end DateTime.</param>
    /// <returns>True if between; otherwise, false.</returns>
    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }
}
