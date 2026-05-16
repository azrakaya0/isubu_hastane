using Hospital.Client.Branding;
using Hospital.Client.Services;
using Hospital.Shared.Enums;

namespace Hospital.Client;

public partial class AdminHomePage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();

    public AdminHomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var branding = await BrandingBootstrap.GetAsync();
        BrandingBootstrap.Apply(AdminHomeBackdrop, null, branding.AdminHomeBannerImage);

        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        StatusMessageLabel.Text = "Veriler güncelleniyor...";
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var patients = await _api.GetPatientsAsync(null, null);
            var doctors = await _api.GetDoctorsAsync(null, null);
            var appointments = await _api.GetAppointmentsAsync(null, null, null, today, tomorrow, AppointmentStatus.Scheduled);
            var reports = await _api.GetLabReportsAsync(null);

            PatientsCountLabel.Text = patients.Count.ToString();
            DoctorsCountLabel.Text = doctors.Count.ToString();
            TodayAppointmentsLabel.Text = appointments.Count.ToString();
            PendingReportsLabel.Text = reports.Count.ToString();
            StatusMessageLabel.Text = $"Son güncelleme: {DateTime.Now:dd.MM.yyyy HH:mm}";
        }
        catch (Exception ex)
        {
            StatusMessageLabel.Text = $"Panoda veri alınamadı: {ex.Message}";
        }
    }

    private async void OnPatientsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("PatientsPage");
    private async void OnDoctorsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("DoctorsPage");
    private async void OnClinicsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("ClinicsPage");
    private async void OnAppointmentsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("AppointmentsPage");
    private async void OnLabReportsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("AdminLabReportsPage");
    private async void OnDutiesClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("AdminDoctorDutiesPage");
    private async void OnPermissionsClicked(object? sender, EventArgs e) => await ShellFlyoutNavigator.GoToAsync("AdminPermissionsPage");
}
