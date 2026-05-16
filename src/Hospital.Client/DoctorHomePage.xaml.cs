using Hospital.Client.Branding;
using Hospital.Client.Services;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class DoctorHomePage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();

    public DoctorHomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var branding = await BrandingBootstrap.GetAsync();
        BrandingBootstrap.Apply(DoctorHomeBackdrop, null, branding.DoctorHomeBannerImage);

        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        DoctorStatusLabel.Text = "Veriler güncelleniyor...";
        try
        {
            var me = await _api.GetDoctorPortalMeAsync();
            var appointments = await _api.GetDoctorPortalAppointmentsAsync();
            var now = DateTime.Now;
            var today = DateTime.Today;

            DoctorNameLabel.Text = me is null ? "Dr. -" : $"Dr. {me.FirstName} {me.LastName}";
            DoctorClinicLabel.Text = me is null ? "Poliklinik bilgisi bulunamadı." : $"{me.ClinicName} • {me.Specialty}";
            DoctorTodayLabel.Text = appointments.Count(x => x.ScheduledAt.Date == today && x.Status == AppointmentStatus.Scheduled).ToString();
            DoctorUpcomingLabel.Text = appointments.Count(x => x.ScheduledAt >= now && x.ScheduledAt <= now.AddDays(7) && x.Status == AppointmentStatus.Scheduled).ToString();
            DoctorCompletedLabel.Text = appointments.Count(x => x.Status == AppointmentStatus.Completed).ToString();
            DoctorStatusLabel.Text = $"Son güncelleme: {DateTime.Now:dd.MM.yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            DoctorStatusLabel.Text = $"Veri alınamadı: {ex.Message}";
        }
    }

    private async void OnCalendarClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorCalendarPage");
    private async void OnAppointmentsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorAppointmentsPage");
    private async void OnBookClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorBookAppointmentPage");
    private async void OnPatientLabsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorPatientLabsPage");
    private async void OnTimelineClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorTimelinePage");
    private async void OnProfileClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorProfilePage");
}
