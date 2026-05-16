using Hospital.Client.Ui;
using Hospital.Shared.Dtos;

namespace Hospital.Client;

public partial class LabReportDetailPage : ContentPage
{
    public LabReportDetailPage(LabReportDto report)
    {
        InitializeComponent();
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

        MetaLabel.Text = $"Kayıt no: {r.Id} • Kayıt tarihi: {r.CreatedAt:g}";
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
