using Hospital.Client.Services;

namespace Hospital.Client.Ui;

internal static class LabReportPdfHelper
{
    private const int MaxPdfBytes = 5 * 1024 * 1024;

    public static async Task<(string? Base64, string? FileName)> PickPdfAsync(Page page)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Tetkik PDF dosyası",
            FileTypes = FilePickerFileType.Pdf
        });

        if (result is null)
        {
            return (null, null);
        }

        await using var stream = await result.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        if (ms.Length > MaxPdfBytes)
        {
            await page.DisplayAlert("PDF", "Dosya en fazla 5 MB olabilir.", "Tamam");
            return (null, null);
        }

        return (Convert.ToBase64String(ms.ToArray()), result.FileName);
    }

    public static async Task OpenPdfAsync(
        Page page,
        IHospitalApiClient api,
        int reportId,
        bool patientPortal,
        string? suggestedFileName)
    {
        try
        {
            var stream = await api.GetLabReportPdfStreamAsync(reportId, patientPortal);
            if (stream is null)
            {
                await page.DisplayAlert("PDF", "Dosya bulunamadı.", "Tamam");
                return;
            }

            await using (stream)
            {
                var name = string.IsNullOrWhiteSpace(suggestedFileName)
                    ? $"tetkik-{reportId}.pdf"
                    : suggestedFileName;
                var path = Path.Combine(FileSystem.CacheDirectory, name);
                await using var file = File.Create(path);
                await stream.CopyToAsync(file);
                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(path)
                });
            }
        }
        catch (Exception ex)
        {
            await page.DisplayAlert("PDF", ex.Message, "Tamam");
        }
    }
}
