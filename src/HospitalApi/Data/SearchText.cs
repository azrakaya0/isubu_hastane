using System.Globalization;

namespace HospitalApi.Data;

/// <summary>
/// Arama sorgularında büyük/küçük harf duyarsız eşleşme için metin normalleştirme.
/// </summary>
internal static class SearchText
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToLower(Turkish);
}
