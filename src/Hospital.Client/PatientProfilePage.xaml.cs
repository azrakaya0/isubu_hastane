using Hospital.Client.Services;
using Hospital.Shared.Dtos;

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

                Bind(me);
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }

    private void Bind(PatientDto me)
    {
        FullNameLabel.Text = $"{me.FirstName} {me.LastName}";
        NationalIdLabel.Text = $"T.C. kimlik no (değiştirilemez): {me.NationalId}";
        BirthLabel.Text = $"Doğum tarihi: {me.BirthDate:d}";
        PhoneEntry.Text = me.Phone;
        EmailEntry.Text = me.Email ?? string.Empty;
        EmergencyNameEntry.Text = me.EmergencyContactName ?? string.Empty;
        EmergencyRelationEntry.Text = me.EmergencyContactRelation ?? string.Empty;
        EmergencyPhoneEntry.Text = me.EmergencyContactPhone ?? string.Empty;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                var updated = await _api.UpdatePatientPortalProfileAsync(new UpdatePatientProfileRequest
                {
                    Phone = PhoneEntry.Text?.Trim(),
                    Email = string.IsNullOrWhiteSpace(EmailEntry.Text) ? null : EmailEntry.Text.Trim(),
                    EmergencyContactName = string.IsNullOrWhiteSpace(EmergencyNameEntry.Text)
                        ? null
                        : EmergencyNameEntry.Text.Trim(),
                    EmergencyContactRelation = string.IsNullOrWhiteSpace(EmergencyRelationEntry.Text)
                        ? null
                        : EmergencyRelationEntry.Text.Trim(),
                    EmergencyContactPhone = string.IsNullOrWhiteSpace(EmergencyPhoneEntry.Text)
                        ? null
                        : EmergencyPhoneEntry.Text.Trim()
                });

                if (updated is null)
                {
                    ErrorLabel.Text = "Kayıt güncellenemedi.";
                    ErrorLabel.IsVisible = true;
                    return;
                }

                Bind(updated);
                await DisplayAlert("Bilgilerim", "İletişim bilgileriniz kaydedildi.", "Tamam");
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.IsVisible = true;
            }
        });
    }
}
