namespace Hospital.Client.Portal;

/// <summary>
/// Windows: Shell/FlyoutPage yerine NavigationPage (WinUI 0xC000027B çökmesini önler).
/// </summary>
internal static class PortalWindowsNavigation
{
    internal static bool UseNavigationPage => DeviceInfo.Platform == DevicePlatform.WinUI;

    internal static Page CreateBootstrap(PortalKind kind) => new PortalBootstrapPage(kind);

    internal static NavigationPage CreatePortal(PortalKind kind)
    {
        PortalRouteTable.Activate(kind);

        var home = PortalRouteTable.CreatePage(PortalRouteTable.DefaultRoute)
            ?? throw new InvalidOperationException("Portal ana sayfası oluşturulamadı.");

        var navigation = new NavigationPage(home)
        {
            BackgroundColor = Color.FromArgb("#F5FAFF"),
            BarBackgroundColor = Color.FromArgb("#1565C0"),
            BarTextColor = Colors.White,
            Title = WindowTitle(kind)
        };

        PortalRouteTable.AttachNavigation(navigation);
        return navigation;
    }

    internal static string WindowTitle(PortalKind kind) =>
        kind switch
        {
            PortalKind.Doctor => "Doktor portalı",
            PortalKind.Patient => "Hasta portalı",
            _ => "Hastane Yönetimi"
        };
}
