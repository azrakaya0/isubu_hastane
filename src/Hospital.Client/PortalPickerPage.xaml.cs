using Hospital.Client.Branding;

namespace Hospital.Client;

public partial class PortalPickerPage : ContentPage
{
    public PortalPickerPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var branding = await BrandingBootstrap.GetAsync();
            BrandingBootstrap.Apply(PortalHeroImage, null, branding.PortalHeroImage);
            BrandingBootstrap.Apply(PortalStaffCardImage, PortalStaffCardFallback, branding.PortalStaffCardImage);
            BrandingBootstrap.Apply(PortalDoctorCardImage, PortalDoctorCardFallback, branding.PortalDoctorCardImage);
            BrandingBootstrap.Apply(PortalPatientCardImage, PortalPatientCardFallback, branding.PortalPatientCardImage);
        }
        catch
        {
            PortalHeroImage.IsVisible = false;
        }
    }

    private async void OnStaffPortalTapped(object? sender, TappedEventArgs e)
    {
        await OpenLoginAsync(LoginPortalKind.Staff);
    }

    private async void OnDoctorPortalTapped(object? sender, TappedEventArgs e)
    {
        await OpenLoginAsync(LoginPortalKind.Doctor);
    }

    private async void OnPatientPortalTapped(object? sender, TappedEventArgs e)
    {
        await OpenLoginAsync(LoginPortalKind.Patient);
    }

    private async Task OpenLoginAsync(LoginPortalKind kind)
    {
        if (Navigation is null)
        {
            await DisplayAlert("Gezinme", "Sayfa geçmişi hazır değil. Uygulamayı yeniden başlatın.", "Tamam");
            return;
        }

        try
        {
            var page = new LoginPage { PortalKind = kind };
            NavigationPage.SetHasNavigationBar(page, true);
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Açılamadı", ex.Message, "Tamam");
        }
    }
}
