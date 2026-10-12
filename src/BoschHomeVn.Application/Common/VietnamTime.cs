namespace BoschHomeVn.Application.Common;

// Shop ở Việt Nam: ngày giờ hiển thị và lọc theo giờ Việt Nam (UTC+7, không đổi giờ mùa hè); DB lưu UTC
public static class VietnamTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    public static DateTime FromUtc(DateTime utc) => utc + Offset;

    // 00:00 giờ Việt Nam của ngày `day`, đổi ra UTC (để so với cột timestamptz)
    public static DateTime StartOfDayUtc(DateOnly day) =>
        DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue) - Offset, DateTimeKind.Utc);
}
