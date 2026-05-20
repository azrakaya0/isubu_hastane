namespace Hospital.Shared;

/// <summary>
/// Randevu slot kuralları: hafta içi 08:30–16:30, öğle 12:00–13:00 hariç, 15 dk aralık; cumartesi-pazar kapalı.
/// </summary>
public static class AppointmentScheduling
{
    public static readonly TimeSpan DayStart = new(8, 30, 0);
    public static readonly TimeSpan DayEnd = new(16, 30, 0);
    public static readonly TimeSpan LunchStart = new(12, 0, 0);
    public static readonly TimeSpan LunchEnd = new(13, 0, 0);
    public const int SlotMinutes = 15;

    public static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsWithinBusinessHours(TimeSpan time) =>
        time >= DayStart && time < DayEnd && (time < LunchStart || time >= LunchEnd);

    public static bool IsAlignedToSlot(DateTime scheduledAt) =>
        scheduledAt.Second == 0 &&
        scheduledAt.Millisecond == 0 &&
        scheduledAt.Minute % SlotMinutes == 0;

    public static bool IsValidSlot(DateTime scheduledAt) =>
        !IsWeekend(scheduledAt.Date) &&
        IsWithinBusinessHours(scheduledAt.TimeOfDay) &&
        IsAlignedToSlot(scheduledAt);

    public static string? ValidateSlotMessage(DateTime scheduledAt)
    {
        if (IsWeekend(scheduledAt.Date))
        {
            return "Cumartesi ve pazar günleri randevu alınamaz.";
        }

        if (!IsWithinBusinessHours(scheduledAt.TimeOfDay))
        {
            return "Randevu saati 08:30–16:30 arasında ve 12:00–13:00 öğle arası dışında olmalıdır.";
        }

        if (!IsAlignedToSlot(scheduledAt))
        {
            return "Randevu saati 15 dakikalık dilimlere oturmalıdır (ör. 09:00, 09:15).";
        }

        return null;
    }

    public static IEnumerable<DateTime> EnumerateSlotsForDate(DateTime date)
    {
        if (IsWeekend(date))
        {
            yield break;
        }

        var day = date.Date;
        for (var t = DayStart; t < DayEnd; t = t.Add(TimeSpan.FromMinutes(SlotMinutes)))
        {
            if (t >= LunchStart && t < LunchEnd)
            {
                continue;
            }

            yield return day + t;
        }
    }

    public static DateTime NormalizeToNextWeekday(DateTime date)
    {
        var d = date.Date;
        while (IsWeekend(d))
        {
            d = d.AddDays(1);
        }

        return d;
    }

    public static string FormatSlotLabel(DateTime slot) => slot.ToString("HH:mm");
}
