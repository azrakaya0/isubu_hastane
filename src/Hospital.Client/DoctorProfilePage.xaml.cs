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
                var clinicLine = string.IsNullOrWhiteSpace(me.ClinicNumber)
                    ? me.ClinicName
                    : $"{me.ClinicName} · No {me.ClinicNumber}";
                ClinicLabel.Text = $"Poliklinik: {clinicLine}";
                OfficeLabel.Text = string.IsNullOrWhiteSpace(me.OfficeLocation)
                    ? "Oda / kat: (yönetim tarafından tanımlanmadı)"
                    : $"Oda / kat: {me.OfficeLocation}";
                PhoneLabel.Text = string.IsNullOrWhiteSpace(me.PublicPhone)
                    ? "Dahili / telefon: —"
                    : $"Dahili / telefon: {me.PublicPhone}";
                EmailLabel.Text = string.IsNullOrWhiteSpace(me.PublicEmail)
                    ? "Kurumsal e-posta: —"
                    : $"Kurumsal e-posta: {me.PublicEmail}";
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }
}
