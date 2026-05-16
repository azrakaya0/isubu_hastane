using Hospital.Client.Services;

namespace Hospital.Client;

public partial class DoctorProfilePage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();

    public DoctorProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var me = await _api.GetDoctorPortalMeAsync();
                if (me is null)
                {
                    ErrorLabel.Text = "Profil bilgisi alınamadı.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                FullNameLabel.Text = $"{me.FirstName} {me.LastName}";
                SpecialtyLabel.Text = me.Specialty;
                ClinicLabel.Text = $"Poliklinik: {me.ClinicName}";
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }

    private async void OnBookForPatientClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new DoctorBookAppointmentPage());

    private async void OnTimelineClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new DoctorTimelinePage());

    private async void OnPatientLabsClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new DoctorPatientLabsPage());
}
