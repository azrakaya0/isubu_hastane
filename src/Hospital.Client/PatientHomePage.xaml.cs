using Hospital.Client.Branding;
using Hospital.Client.Services;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class PatientHomePage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();

    public PatientHomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var branding = await BrandingBootstrap.GetAsync();
        BrandingBootstrap.Apply(PatientHomeBackdrop, null, branding.PatientHomeBannerImage);

        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        PatientStatusLabel.Text = "Veriler güncelleniyor...";
        try
        {
            var me = await _api.GetPatientPortalMeAsync();
            var appointments = await _api.GetPatientPortalAppointmentsAsync();
            var labs = await _api.GetPatientPortalLabReportsAsync();
            var now = DateTime.Now;

            PatientNameLabel.Text = me is null ? "Hasta profili" : $"{me.FirstName} {me.LastName}";
            PatientIdentityLabel.Text = me is null ? "Kimlik bilgisi okunamadı" : $"T.C.: {me.NationalId}";
            PatientUpcomingLabel.Text = appointments.Count(x => x.ScheduledAt >= now && x.Status == AppointmentStatus.Scheduled).ToString();
            PatientCompletedLabel.Text = appointments.Count(x => x.Status == AppointmentStatus.Completed).ToString();
            PatientLabCountLabel.Text = labs.Count.ToString();
            PatientStatusLabel.Text = $"Son güncelleme: {DateTime.Now:dd.MM.yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            PatientStatusLabel.Text = $"Veri alınamadı: {ex.Message}";
        }
    }

    private async void OnBookClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("PatientBookAppointmentPage");
    private async void OnAppointmentsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("PatientAppointmentsPage");
    private async void OnTimelineClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("PatientTimelinePage");
    private async void OnLabsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("PatientLabReportsPage");
    private async void OnProfileClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("PatientProfilePage");
}
