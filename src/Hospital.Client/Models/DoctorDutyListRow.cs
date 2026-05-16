using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client.Models;

public sealed class DoctorDutyListRow
{
    public DoctorDutyDto Source { get; }

    public DoctorDutyListRow(DoctorDutyDto source)
    {
        Source = source;
        var (bg, fg) = DutyPresentation.KindAccent(source.DutyKind);
        KindBadgeBg = bg;
        KindBadgeFg = fg;
    }

    public string DoctorFullName => Source.DoctorFullName;

    public string ClinicName => Source.ClinicName;

    public DateTime DutyDate => Source.DutyDate;

    public string DutyKind => Source.DutyKind;

    public bool HasNotes => !string.IsNullOrWhiteSpace(Source.Notes);

    public string? NotesSnippet =>
        string.IsNullOrWhiteSpace(Source.Notes) ? null :
        Source.Notes.Trim().Length > 200 ? Source.Notes.Trim()[..197] + "…" : Source.Notes.Trim();

    public Color KindBadgeBg { get; }

    public Color KindBadgeFg { get; }
}
