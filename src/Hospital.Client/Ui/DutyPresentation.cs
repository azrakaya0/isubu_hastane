namespace Hospital.Client.Ui;

public static class DutyPresentation
{
    public static (Color Bg, Color Fg) KindAccent(string? kind)
    {
        var k = (kind ?? string.Empty).Trim().ToLowerInvariant();

        if (k.Contains("izin", StringComparison.Ordinal) || k.Contains("raporlu", StringComparison.Ordinal))
        {
            return (Color.FromArgb("#FFF8E1"), Color.FromArgb("#F57F17"));
        }

        if (k.Contains("nöbet", StringComparison.Ordinal) || k.Contains("nobet", StringComparison.Ordinal))
        {
            return (Color.FromArgb("#E8EAF6"), Color.FromArgb("#3949AB"));
        }

        if (k.Contains("mesai", StringComparison.Ordinal))
        {
            return (Color.FromArgb("#E3F2FD"), Color.FromArgb("#1565C0"));
        }

        return (Color.FromArgb("#ECEFF1"), Color.FromArgb("#455A64"));
    }
}
