using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Hospital.Client.Branding;

internal static class BrandingBootstrap
{
    private static readonly object Sync = new();

    /// <remarks>Önbelleği paylaşılan tamamlanmış görev.</remarks>
    private static Task<BrandingOptions>? LoadTask;

    internal static BrandingOptions? Cached { get; private set; }

    internal static Task<BrandingOptions> GetAsync()
    {
        lock (Sync)
        {
            LoadTask ??= SafeLoadAsync();
            return LoadTask;
        }
    }

    internal static void Apply(Image? img, VisualElement? fallback, string? fileNameOnly)
    {
        if (img is null && fallback is null)
        {
            return;
        }

        var cleaned = NormalizeFile(fileNameOnly);
        if (cleaned is null)
        {
            if (fallback is not null)
            {
                fallback.IsVisible = true;
            }

            if (img is not null)
            {
                img.IsVisible = false;
                img.Source = null;
            }

            return;
        }

        if (img is not null)
        {
            img.Source = ImageSource.FromFile(cleaned);
            img.IsVisible = true;
        }

        if (fallback is not null)
        {
            fallback.IsVisible = false;
        }
    }

    private static string? NormalizeFile(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var name = Path.GetFileName(value.Trim());
        if (string.IsNullOrEmpty(name) || name.IndexOf('/') >= 0 || name.IndexOf('\\') >= 0)
        {
            return null;
        }

        if ((name.Length > 0 && name[0] == '.') || string.Equals(name, "..", StringComparison.Ordinal))
        {
            return null;
        }

        return name;
    }

    private static async Task<BrandingOptions> SafeLoadAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("branding.json");
            using var sr = new StreamReader(stream);
            var json = await sr.ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<BrandingOptions>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            }) ?? new BrandingOptions();

            Cached = dto;
            return dto;
        }
        catch (Exception ex) when (
            ex is FileNotFoundException
            || ex is DirectoryNotFoundException
            || ex is IOException
            || ex is JsonException)
        {
            Cached ??= new BrandingOptions();
            return Cached;
        }
    }
}
