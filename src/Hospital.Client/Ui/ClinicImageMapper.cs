using System.Globalization;

namespace Hospital.Client.Ui;

/// <summary>
/// Seed poliklinik adlarıyla Resources/Images altındaki clinic_*.jpg dosyalarını eşler.
/// </summary>
internal static class ClinicImageMapper
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    internal static string? TryResolveImage(string? clinicName)
    {
        var raw = (clinicName ?? string.Empty).Trim();
        if (raw.Length == 0)
        {
            return null;
        }

        var key = raw.ToLower(Tr);

        // Veritabanı seed’leriyle doğrudan uyumlu isimler
        switch (key)
        {
            case "dahiliye":
                return "clinic_internal.jpg";
            case "kardiyoloji":
                return "clinic_cardiology.jpg";
            case "ortopedi":
                return "clinic_orthopedics.jpg";
            case "pediatri":
                return "clinic_pediatrics.jpg";
        }

        if (ContainsAny(key,
                ["jinekoloji", "kadın doğum", "kadın hast", "kadın sağlık", "ginekoloji", "gebelik"]))
        {
            return "clinic_gynecology.jpg";
        }

        if (ContainsAny(key,
                ["radyolo", "görüntüleme merkezi", "nükleer tıp", "bt merkezi", "mr birim"]))
        {
            return "clinic_radiology.jpg";
        }

        return null;
    }

    private static bool ContainsAny(string haystack, IEnumerable<string> needles)
    {
        foreach (var nd in needles)
        {
            if (haystack.Contains(nd, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
