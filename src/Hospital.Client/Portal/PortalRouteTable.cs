namespace Hospital.Client.Portal;

internal enum PortalKind
{
    Admin,
    Doctor,
    Patient
}

/// <summary>
/// Windows NavigationPage gezinmesi için rota tablosu.
/// </summary>
internal static class PortalRouteTable
{
    private static readonly IReadOnlyDictionary<string, PortalMenuItem> Empty =
        new Dictionary<string, PortalMenuItem>(StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, PortalMenuItem> _routes = Empty;
    private static NavigationPage? _navigation;

    internal static NavigationPage? Navigation => _navigation;

    internal static void Activate(PortalKind kind)
    {
        var items = kind switch
        {
            PortalKind.Doctor => PortalMenus.DoctorItems,
            PortalKind.Patient => PortalMenus.PatientItems,
            _ => PortalMenus.AdminItems
        };

        _routes = items.ToDictionary(i => i.Route, StringComparer.Ordinal);
    }

    internal static string DefaultRoute =>
        _routes.Values.FirstOrDefault()?.Route ?? "AdminHomePage";

    internal static IReadOnlyList<PortalMenuItem> MenuItems => _routes.Values.ToList();

    internal static Page? CreatePage(string route)
    {
        if (!_routes.TryGetValue(route, out var item))
        {
            return null;
        }

        try
        {
            var page = item.CreatePage();
            if (page is null)
            {
                return CreateErrorPage(route, "Sayfa oluşturulamadı.");
            }

            page.Title = item.Title;
            return page;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sayfa oluşturma hatası ({route}): {ex}");
            return CreateErrorPage(route, ex.Message);
        }
    }

    private static Page CreateErrorPage(string route, string errorMessage)
    {
        return new ContentPage
        {
            Title = route,
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new Label { Text = "Sayfa yüklenemedi.", FontAttributes = FontAttributes.Bold, FontSize = 18 },
                        new Label { Text = route, FontSize = 14, TextColor = Colors.Red },
                        new Label { Text = errorMessage, FontSize = 14 }
                    }
                }
            }
        };
    }

    internal static void AttachNavigation(NavigationPage navigation) => _navigation = navigation;

    internal static void Clear()
    {
        _navigation = null;
        _routes = Empty;
    }
}
