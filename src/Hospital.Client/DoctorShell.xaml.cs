using Hospital.Client.Services;

namespace Hospital.Client;

public partial class DoctorShell : Shell
{
    public DoctorShell()
    {
        InitializeComponent();
        Loaded += OnShellLoaded;
    }

    private void OnShellLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnShellLoaded;
        FlyoutFooterVersion.Text =
            $"Sürüm {AppInfo.Current.VersionString} · Yapı {AppInfo.Current.BuildString}";
    }

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        var store = AppLocator.Services.GetRequiredService<IAuthTokenStore>();
        store.Clear();
        Application.Current!.Windows[0].Page = App.CreateLoginNavigation();
    }
}
