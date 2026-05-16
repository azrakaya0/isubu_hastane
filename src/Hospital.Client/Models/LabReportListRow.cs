using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client.Models;

public sealed class LabReportListRow
{
    public LabReportDto Source { get; }

    public LabReportListRow(LabReportDto source)
    {
        Source = source;
        var (bg, fg) = LabPresentation.CategoryAccent(source.Category);
        CategoryBadgeBg = bg;
        CategoryBadgeFg = fg;
        IsCriticalAttention = LabPresentation.IsLikelyCritical(source.Summary, source.Title);
    }

    public string Title => Source.Title;

    public string PatientName => Source.PatientFullName;

    public string Category => Source.Category;

    public string Summary => Source.Summary;

    public DateTime ResultDate => Source.ResultDate;

    public string? OrderingDoctorName => Source.OrderingDoctorName;

    public Color CategoryBadgeBg { get; }

    public Color CategoryBadgeFg { get; }

    public bool IsCriticalAttention { get; }
}
