namespace Hospital.Client.Portal;

/// <summary>
/// Windows: Giriş sayfasından ağır portala geçerken ara adım (WinUI MainPage çökmesini azaltır).
/// </summary>
internal sealed class PortalBootstrapPage : ContentPage
{
    private readonly PortalKind _kind;
    private bool _started;

    public PortalBootstrapPage(PortalKind kind)
    {
        _kind = kind;
        Title = PortalWindowsNavigation.WindowTitle(kind);
        BackgroundColor = Color.FromArgb("#F5FAFF");
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(40),
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Color.FromArgb("#1565C0"),
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Portal açılıyor…",
                    FontSize = 15,
                    TextColor = Color.FromArgb("#1A2B3C"),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_started)
        {
            return;
        }

        _started = true;

        try
        {
            await Task.Delay(250);
            if (Application.Current is not { } app)
            {
                return;
            }

            app.MainPage = PortalWindowsNavigation.CreatePortal(_kind);
        }
        catch (Exception ex)
        {
            CrashLogger.Log(ex, "PortalBootstrapPage");
            try
            {
                await DisplayAlert("Hata", $"Portal açılamadı: {ex.Message}", "Tamam");
            }
            catch
            {
                // ignore
            }

            await AppNavigator.ReturnToLoginAsync();
        }
    }
}
