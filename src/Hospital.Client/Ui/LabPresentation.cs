namespace Hospital.Client.Ui;

public static class LabPresentation
{
    private static readonly (string[] Keys, Color Bg, Color Fg)[] CategoryPalette =
    {
        (["rad", "film", "görü", "manyetik", "bt ", " mr", " mr.", " ultrason"], Color.FromArgb("#E0F2F1"), Color.FromArgb("#00695C")),
        (["kardiyo", "ekg", "eko", "holter"], Color.FromArgb("#FFEBEE"), Color.FromArgb("#C62828")),
        (["patoloji", "biyopsi"], Color.FromArgb("#F3E5F5"), Color.FromArgb("#6A1B9A")),
        (["micro", "bakteri", "kültür"], Color.FromArgb("#E8F5E9"), Color.FromArgb("#1B5E20")),
        (["endos", "gastro"], Color.FromArgb("#FFF8E1"), Color.FromArgb("#F57F17")),
        (["nöroloji", "eeg"], Color.FromArgb("#EDE7F6"), Color.FromArgb("#4527A0")),
        (["kan ", " hem", "tam kan", "biyokimya", "laboratuvar", "lab"], Color.FromArgb("#E3F2FD"), Color.FromArgb("#0D47A1")),
        (["idrar", "i̇drar"], Color.FromArgb("#E1F5FE"), Color.FromArgb("#01579B"))
    };

    /// <summary>
    /// Türkçe/İngilizce düşük büyük harf duyarlısı kategori rengi.
    /// </summary>
    public static (Color BadgeBackground, Color BadgeForeground) CategoryAccent(string? category)
    {
        var c = (category ?? string.Empty).Trim();
        if (c.Length == 0)
        {
            return (Color.FromArgb("#ECEFF1"), Color.FromArgb("#455A64"));
        }

        var norm = Normalize(c);

        foreach (var row in CategoryPalette)
        {
            foreach (var key in row.Keys)
            {
                if (norm.Contains(Normalize(key), StringComparison.OrdinalIgnoreCase))
                {
                    return (row.Bg, row.Fg);
                }
            }
        }

        return (Color.FromArgb("#FFF3E0"), Color.FromArgb("#E65100"));
    }

    private static string Normalize(string s) =>
        s.Trim().ToLower(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));

    private static readonly string[] CriticalPhrases =
    [
        "kritik",
        "acil",
        "⚠",
        "**",
        "ciddi",
        "risk",
        "yüksek risk",
        "yüksek tehlike",
        "pozitif",
        "patoloji",
        "patolojik",
        "malign",
        "anormal",
        "hayati",
        "dikkat"
    ];

    public static bool IsLikelyCritical(string? summary, string? title)
    {
        var blob = $"{title} {summary}".Trim();
        if (blob.Length == 0)
        {
            return false;
        }

        var lowered = blob.ToLowerInvariant();
        foreach (var p in CriticalPhrases)
        {
            if (lowered.Contains(p, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
