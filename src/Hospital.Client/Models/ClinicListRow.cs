using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client.Models;

public sealed class ClinicListRow
{
    public ClinicDto Source { get; }

    public ClinicListRow(ClinicDto source)
    {
        Source = source;
        ThumbAsset = ClinicImageMapper.TryResolveImage(source.Name);
    }

    public string Name => Source.Name;

    public string Description => Source.Description ?? string.Empty;

    public string? ThumbAsset { get; }

    public bool HasThumb => ThumbAsset is not null;

    /// <remarks>Tırnak/avantaj harfi — görsel olmayan kartlarda</remarks>
    public string NameBadge =>
        string.IsNullOrWhiteSpace(Source.Name)
            ? "?"
            : char.ToUpper(Source.Name.Trim()[0], new System.Globalization.CultureInfo("tr-TR")).ToString();

    /// <remarks>Liste/görünür metin gerekiyorsa</remarks>
    public bool NeedsPlaceholder => !HasThumb;

    public ImageSource? ThumbSource =>
        ThumbAsset is { } file ? ImageSource.FromFile(file) : null;
}
