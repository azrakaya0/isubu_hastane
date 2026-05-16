using System.Text.RegularExpressions;
using Hospital.Client.Branding;
using Hospital.Shared.Dtos;
using Hospital.Client.Services;

namespace Hospital.Client;

public partial class LoginPage : ContentPage
{
    private readonly IHospitalApiClient _api = AppLocator.Services.GetRequiredService<IHospitalApiClient>();
    private bool _loginInProgress;
    private bool _passwordVisible;

    public LoginPortalKind PortalKind { get; set; } = LoginPortalKind.Staff;

    public LoginPage()
    {
        InitializeComponent();
        Loaded += OnLoginPageLoaded;
    }

    private async void OnLoginPageLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoginPageLoaded;
        ApplyPortalUi();

        try
        {
            var branding = await BrandingBootstrap.GetAsync();
            BrandingBootstrap.Apply(LoginLogoImage, LoginLogoFallback, branding.LoginLogoImage);
        }
        catch
        {
            LoginLogoImage.IsVisible = false;
            LoginLogoFallback.IsVisible = true;
        }
    }

    private void ApplyPortalUi()
    {
        switch (PortalKind)
        {
            case LoginPortalKind.Doctor:
                Title = "Doktor girişi";
                PortalTitleLabel.Text = "Doktor portalı";
                PortalSubtitleLabel.Text = "Yalnızca kendi randevu listenize erişirsiniz; yönetim işlemleri kurumsal hesaptadır.";
                IdentifierLabel.Text = "Portal kullanıcı adı";
                UserNameEntry.Placeholder = "ör. dr.ayilmaz";
                UserNameEntry.Keyboard = Keyboard.Text;
                UserNameEntry.MaxLength = 64;
                UserNameEntry.Text = "dr.ayilmaz";
                PortalHintLabel.Text = "Örnek: dr.ayilmaz veya dr.mkaya — şifre Doctor123!";
                break;
            case LoginPortalKind.Patient:
                Title = "Hasta girişi";
                PortalTitleLabel.Text = "Hasta portalı";
                PortalSubtitleLabel.Text = "Kayıtlı T.C. kimlik numaranız ve portal şifreniz ile kendi bilgilerinize erişin.";
                IdentifierLabel.Text = "T.C. kimlik numarası";
                UserNameEntry.Placeholder = "11 haneli T.C. kimlik no";
                UserNameEntry.Keyboard = Keyboard.Default;
                UserNameEntry.MaxLength = 11;
                UserNameEntry.Text = "12345678901";
                PortalHintLabel.Text = "Örnek T.C.: 12345678901 — şifre Patient123!";
                break;
            default:
                Title = "Yönetim girişi";
                PortalTitleLabel.Text = "Kurumsal giriş";
                PortalSubtitleLabel.Text =
                    "Hasta kayıtları, poliklinikler, doktorlar ve randevu yönetimi için güvenli oturum açın.";
                IdentifierLabel.Text = "Kullanıcı adı";
                UserNameEntry.Placeholder = "Kurumsal kullanıcı adınız";
                UserNameEntry.Keyboard = Keyboard.Text;
                UserNameEntry.MaxLength = 64;
                UserNameEntry.Text = "admin";
                PortalHintLabel.Text = "Test hesabı: admin / Admin123!";
                break;
        }
    }

    private void OnTogglePasswordVisibility(object? sender, EventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        PasswordEntry.IsPassword = !_passwordVisible;
        PasswordVisibilityButton.Text = _passwordVisible ? "Gizle" : "Göster";
    }

    private void HideLoginError()
    {
        ErrorBanner.IsVisible = false;
    }

    private void ShowLoginError(string message)
    {
        ErrorBannerMessage.Text = message;
        ErrorBanner.IsVisible = true;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await AttemptLoginAsync();
    }

    private async void OnLoginRetryClicked(object? sender, EventArgs e)
    {
        HideLoginError();
        await AttemptLoginAsync();
    }

    private async Task AttemptLoginAsync()
    {
        if (_loginInProgress)
        {
            return;
        }

        HideLoginError();

        if (!AcceptTermsCheckBox.IsChecked)
        {
            await DisplayAlert("Doğrulama", "Devam etmek için koşulları kabul etmelisiniz.", "Tamam");
            return;
        }

        var userName = UserNameEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Doğrulama", "Giriş bilgileri boş olamaz.", "Tamam");
            return;
        }

        if (PortalKind == LoginPortalKind.Patient)
        {
            if (userName.Length != 11 || !Regex.IsMatch(userName, "^[0-9]{11}$"))
            {
                await DisplayAlert("Doğrulama", "T.C. kimlik numarası 11 rakamdan oluşmalıdır.", "Tamam");
                return;
            }
        }
        else if (userName.Length > 64 || password.Length > 128)
        {
            await DisplayAlert("Doğrulama", "Kullanıcı adı veya şifre çok uzun.", "Tamam");
            return;
        }

        _loginInProgress = true;
        try
        {
            await PageUi.WithSpinnerAsync(BusyOverlay, BusySpinner, async () =>
            {
                try
                {
                    LoginResponse? result = PortalKind switch
                    {
                        LoginPortalKind.Doctor => await _api.LoginDoctorPortalAsync(new DoctorPortalLoginRequest
                        {
                            UserName = userName,
                            Password = password
                        }),
                        LoginPortalKind.Patient => await _api.LoginPatientPortalAsync(new PatientPortalLoginRequest
                        {
                            NationalId = userName,
                            Password = password
                        }),
                        _ => await _api.LoginAsync(new LoginRequest { UserName = userName, Password = password })
                    };

                    if (result is null)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                            ShowLoginError("Bilgiler doğrulanamadı. T.C. no / kullanıcı adı veya şifreyi kontrol edin."));
                        return;
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Application.Current!.MainPage = PortalKind switch
                        {
                            LoginPortalKind.Doctor => new DoctorShell(),
                            LoginPortalKind.Patient => new PatientShell(),
                            _ => new AppShell()
                        };
                    });
                }
                catch (Exception ex)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        ShowLoginError($"Sunucuya erişilemedi: {ex.Message}"));
                }
            });
        }
        finally
        {
            _loginInProgress = false;
        }
    }
}
