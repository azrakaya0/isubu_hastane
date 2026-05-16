using Hospital.Client.Services;

namespace Hospital.Client;

public partial class PatientProfilePage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();

    public PatientProfilePage()
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
                var me = await _api.GetPatientPortalMeAsync();
                if (me is null)
                {
                    ErrorLabel.Text = "Bilgiler alınamadı.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                FullNameLabel.Text = $"{me.FirstName} {me.LastName}";
                NationalIdLabel.Text = $"T.C.: {me.NationalId}";
                PhoneLabel.Text = $"Telefon: {me.Phone}";
                BirthLabel.Text = $"Doğum: {me.BirthDate:d}";
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }

    private async void OnBookAppointmentClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new PatientBookAppointmentPage());

    private async void OnTimelineClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new PatientTimelinePage());

    private async void OnLabsClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new PatientLabReportsPage());
}
