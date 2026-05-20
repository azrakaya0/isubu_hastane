using System.Globalization;

namespace Hospital.Client.Converters;

public sealed class FlyoutMenuIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return ConvertTitle(value as string);
        }
        catch
        {
            return "•";
        }
    }

    private static string ConvertTitle(string? title) =>
        (title ?? string.Empty) switch
        {
            "Ana sayfa" => "⌂",
            "Hastalar" => "◉",
            "Poliklinikler" => "⚕",
            "Doktorlar" => "✚",
            "Randevular" => "▦",
            "Randevularım" => "▦",
            "Randevu al" => "＋",
            "Tetkik raporları" => "◧",
            "Tetkiklerim" => "◧",
            "Nöbet ve izinler" => "◷",
            "Erişim ve roller" => "◎",
            "Sağlık özeti" => "↗",
            "Geçmiş akış" => "↗",
            "Bilgilerim" => "◇",
            "Profilim" => "◇",
            "Hasta tetkikleri" => "◧",
            "Hastaya randevu" => "＋",
            "Takvim" => "▦",
            "Geçmiş" => "↗",
            "Şifre Değiştir" => "🔒",
            _ => "•"
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
