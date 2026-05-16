using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client.Ui;

public static class AppointmentPresentation
{
    public static string StatusLabel(AppointmentStatus status) =>
        status switch
        {
            AppointmentStatus.Scheduled => "Planlı",
            AppointmentStatus.Completed => "Tamamlandı",
            AppointmentStatus.Cancelled => "İptal",
            _ => status.ToString()
        };

    public static Color StatusBadgeBackground(AppointmentStatus status) =>
        status switch
        {
            AppointmentStatus.Scheduled => Color.FromArgb("#E3F2FD"),
            AppointmentStatus.Completed => Color.FromArgb("#E8F5E9"),
            AppointmentStatus.Cancelled => Color.FromArgb("#ECEFF1"),
            _ => Color.FromArgb("#E3F2FD")
        };

    public static Color StatusBadgeForeground(AppointmentStatus status) =>
        status switch
        {
            AppointmentStatus.Scheduled => Color.FromArgb("#0D47A1"),
            AppointmentStatus.Completed => Color.FromArgb("#2E7D32"),
            AppointmentStatus.Cancelled => Color.FromArgb("#546E7A"),
            _ => Color.FromArgb("#0D47A1")
        };

    /// <summary>
    /// Zaman ve duruma göre operasyon önceliği (altyapıya alan eklenmeden türetilmiş).
    /// </summary>
    public static (string Label, Color Bg, Color Fg) Urgency(AppointmentDto dto)
    {
        var now = DateTime.Now;
        if (dto.Status == AppointmentStatus.Cancelled)
        {
            return ("Kapalı", Color.FromArgb("#ECEFF1"), Color.FromArgb("#546E7A"));
        }

        if (dto.Status == AppointmentStatus.Completed)
        {
            return ("Arşiv", Color.FromArgb("#F1F8E9"), Color.FromArgb("#558B2F"));
        }

        if (dto.ScheduledAt < now)
        {
            return ("Takvim gecikti", Color.FromArgb("#FFEBEE"), Color.FromArgb("#B71C1C"));
        }

        var hours = (dto.ScheduledAt - now).TotalHours;
        if (hours <= 4)
        {
            return ("Yakın saat", Color.FromArgb("#FFF3E0"), Color.FromArgb("#E65100"));
        }

        if (dto.ScheduledAt.Date == now.Date)
        {
            return ("Bugün", Color.FromArgb("#FFF8E1"), Color.FromArgb("#F57F17"));
        }

        if (hours <= 72)
        {
            return ("Yakın", Color.FromArgb("#E8EAF6"), Color.FromArgb("#3949AB"));
        }

        return ("Planlı sıra", Color.FromArgb("#E3F2FD"), Color.FromArgb("#1565C0"));
    }
}
