using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client.Models;

public sealed class AppointmentListRow
{
    public AppointmentDto Source { get; }

    public AppointmentListRow(AppointmentDto source)
    {
        Source = source;
        var u = AppointmentPresentation.Urgency(source);
        UrgencyLabel = u.Label;
        UrgencyBadgeBg = u.Bg;
        UrgencyBadgeFg = u.Fg;
        StatusBadgeBg = AppointmentPresentation.StatusBadgeBackground(source.Status);
        StatusBadgeFg = AppointmentPresentation.StatusBadgeForeground(source.Status);
        StatusLabel = AppointmentPresentation.StatusLabel(source.Status);
    }

    public string ScheduledAtText => Source.ScheduledAt.ToString("g");

    public string PatientName => Source.PatientFullName;

    public string DoctorName => Source.DoctorFullName;

    public string ClinicName => Source.ClinicName;

    public string StatusLabel { get; }

    public Color StatusBadgeBg { get; }

    public Color StatusBadgeFg { get; }

    public string UrgencyLabel { get; }

    public Color UrgencyBadgeBg { get; }

    public Color UrgencyBadgeFg { get; }

    public bool HasNotes => !string.IsNullOrWhiteSpace(Source.Notes);

    public string? NotesSnippet =>
        string.IsNullOrWhiteSpace(Source.Notes) ? null :
        Source.Notes.Trim().Length > 120 ? Source.Notes.Trim()[..117] + "…" : Source.Notes.Trim();
}
