using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class ChangePasswordPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();

    public ChangePasswordPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var current = CurrentPasswordEntry.Text ?? string.Empty;
        var next = NewPasswordEntry.Text ?? string.Empty;
        var confirm = ConfirmPasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(next))
        {
            await DisplayAlert("Doğrulama", "Mevcut ve yeni şifre alanları zorunludur.", "Tamam");
            return;
        }

        if (next.Length < 6)
        {
            await DisplayAlert("Doğrulama", "Yeni şifre en az 6 karakter olmalıdır.", "Tamam");
            return;
        }

        if (next != confirm)
        {
            await DisplayAlert("Doğrulama", "Yeni şifre tekrarı eşleşmiyor.", "Tamam");
            return;
        }

        await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
        {
            try
            {
                await _api.ChangePasswordAsync(new ChangePasswordRequest
                {
                    CurrentPassword = current,
                    NewPassword = next
                });
                await DisplayAlert("Tamam", "Şifreniz güncellendi.", "Kapat");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        });
    }
}
