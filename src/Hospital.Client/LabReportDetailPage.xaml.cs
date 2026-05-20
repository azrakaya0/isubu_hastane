using Hospital.Client.Services;
using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class LabReportDetailPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private readonly LabReportDto _report;
    private readonly bool _patientPortal;

    public LabReportDetailPage(LabReportDto report, bool patientPortal = false)
    {
        InitializeComponent();
        _report = report;
        _patientPortal = patientPortal;
        Bind(report);
    }

    private void Bind(LabReportDto r)
    {
        HeroTitle.Text = "Tetkik detayı";
        HeroSubtitle.Text =
            string.IsNullOrWhiteSpace(r.OrderingDoctorName)
                ? $"{r.PatientFullName} • {r.Category}"
                : $"{r.PatientFullName} • İsteyen: {r.OrderingDoctorName}";

        var (bg, fg) = LabPresentation.CategoryAccent(r.Category);
        CategoryRibbon.BackgroundColor = bg;
        CategoryLabel.Text = string.IsNullOrWhiteSpace(r.Category) ? "Genel" : r.Category;
        CategoryLabel.TextColor = fg;

        DateLabel.Text = r.ResultDate.ToString("dd.MM.yyyy");
        TitleLabel.Text = r.Title;
        SummaryLabel.Text = string.IsNullOrWhiteSpace(r.Summary) ? "Özet metni girilmemiş." : r.Summary;

        var critical = LabPresentation.IsLikelyCritical(r.Summary, r.Title);
        CriticalPanel.IsVisible = critical;

        if (!string.IsNullOrWhiteSpace(r.OrderingDoctorName))
        {
            DoctorPanel.IsVisible = true;
            DoctorLabel.Text = r.OrderingDoctorName;
        }

        MetaLabel.Text = r.HasPdf
            ? $"Kayıt no: {r.Id} • PDF ekli • Kayıt tarihi: {r.CreatedAt:g}"
            : $"Kayıt no: {r.Id} • Kayıt tarihi: {r.CreatedAt:g}";

        OpenPdfButton.IsVisible = r.HasPdf;
    }

    private async void OnOpenPdfClicked(object? sender, EventArgs e) =>
        await LabReportPdfHelper.OpenPdfAsync(this, _api, _report.Id, _patientPortal, _report.PdfFileName);

    private async void OnCloseClicked(object? sender, EventArgs e) =>
        await Navigation.PopAsync();
}
