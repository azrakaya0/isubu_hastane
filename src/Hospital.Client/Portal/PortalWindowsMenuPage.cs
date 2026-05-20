using Hospital.Client.Services;

namespace Hospital.Client.Portal;

/// <summary>
/// Windows: FlyoutPage yerine tam menü listesi (NavigationPage içinde).
/// </summary>
internal sealed class PortalWindowsMenuPage : ContentPage
{
    public PortalWindowsMenuPage()
    {
        Title = "Menü";
        BackgroundColor = Color.FromArgb("#F5FAFF");

        var stack = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16),
            Spacing = 8
        };

        foreach (var item in PortalRouteTable.MenuItems)
        {
            var route = item.Route;
            var button = new Button
            {
                Text = item.Title,
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#1A2B3C"),
                FontFamily = "OpenSansSemibold",
                FontSize = 14,
                CornerRadius = 8,
                Padding = new Thickness(14, 12),
                HorizontalOptions = LayoutOptions.Fill,
                BorderWidth = 0
            };
            button.Clicked += async (_, _) =>
            {
                if (Navigation is null)
                {
                    return;
                }

                await Navigation.PopAsync();
                await ShellFlyoutNavigator.GoToAsync(route);
            };
            stack.Add(button);
        }

        var logout = new Button
        {
            Text = "Çıkış yap",
            Margin = new Thickness(0, 16, 0, 0),
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#1565C0"),
            BorderColor = Color.FromArgb("#1565C0"),
            BorderWidth = 1.5,
            CornerRadius = 10,
            Padding = new Thickness(16, 10)
        };
        logout.Clicked += async (_, _) =>
        {
            var store = AppLocator.Services.GetRequiredService<IAuthTokenStore>();
            store.Clear();
            PortalRouteTable.Clear();
            PortalFlyoutHost.Current = null;
            await AppNavigator.ReturnToLoginAsync();
        };

        stack.Add(logout);
        Content = new ScrollView { Content = stack };
    }
}
