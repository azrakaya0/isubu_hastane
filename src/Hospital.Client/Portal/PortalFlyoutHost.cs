using Hospital.Client.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Hospital.Client.Portal;

/// <summary>
/// Shell yerine FlyoutPage — Windows (unpackaged) üzerinde daha kararlı portal gezinmesi.
/// </summary>
internal sealed class PortalFlyoutHost : FlyoutPage
{
    private static readonly Color NavBarColor = Color.FromArgb("#1565C0");

    private readonly IReadOnlyDictionary<string, PortalMenuItem> _items;

    public static PortalFlyoutHost? Current { get; internal set; }

    public PortalFlyoutHost(
        string windowTitle,
        string badge,
        string headline,
        string tagline,
        Color accent,
        IReadOnlyList<PortalMenuItem> items)
    {
        if (items.Count == 0)
        {
            throw new ArgumentException("Portal menüsü boş olamaz.", nameof(items));
        }

        Title = windowTitle;
        _items = items.ToDictionary(i => i.Route, StringComparer.Ordinal);

        FlyoutLayoutBehavior = DeviceInfo.Platform == DevicePlatform.WinUI
            ? FlyoutLayoutBehavior.Popover
            : FlyoutLayoutBehavior.Default;

        BackgroundColor = Color.FromArgb("#F5FAFF");
        IsGestureEnabled = true;

        Flyout = BuildFlyout(badge, headline, tagline, accent, items);
        Detail = CreateDetailNavigation(items[0]);

        Current = this;
    }

    public Task NavigateToAsync(string route)
    {
        if (!_items.TryGetValue(route, out var item))
        {
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            Detail = CreateDetailNavigation(item);
            IsPresented = false;
        });
    }

    private NavigationPage CreateDetailNavigation(PortalMenuItem item)
    {
        Page page;
        try
        {
            page = item.CreatePage();
            if (page is null)
            {
                throw new InvalidOperationException("Sayfa oluşturulamadı.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Portal sayfa oluşturma hatası ({item.Route}): {ex}");
            page = new ContentPage
            {
                Title = item.Title,
                Content = new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Padding = new Thickness(20),
                        Spacing = 12,
                        Children =
                        {
                            new Label { Text = "Sayfa yüklenemedi.", FontAttributes = FontAttributes.Bold, FontSize = 18 },
                            new Label { Text = item.Route, FontSize = 14, TextColor = Colors.Red },
                            new Label { Text = ex.Message, FontSize = 14 }
                        }
                    }
                }
            };
        }

        page.Title = item.Title;
        NavigationPage.SetHasNavigationBar(page, true);

        return new NavigationPage(page)
        {
            BarBackgroundColor = NavBarColor,
            BarTextColor = Colors.White,
            Title = item.Title
        };
    }

    private ContentPage BuildFlyout(
        string badge,
        string headline,
        string tagline,
        Color accent,
        IReadOnlyList<PortalMenuItem> items)
    {
        var menuStack = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(8, 4) };

        foreach (var item in items)
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
            button.Clicked += async (_, _) => await NavigateToAsync(route).ConfigureAwait(false);
            menuStack.Add(button);
        }

        var logoutButton = new Button
        {
            Text = "Çıkış yap",
            Margin = new Thickness(8, 8, 8, 16),
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#1565C0"),
            BorderColor = Color.FromArgb("#1565C0"),
            BorderWidth = 1.5,
            CornerRadius = 10,
            Padding = new Thickness(16, 10)
        };
        logoutButton.Clicked += OnLogoutClicked;

        var versionLabel = new Label
        {
            Text = $"Sürüm {AppInfo.Current.VersionString}",
            FontSize = 11,
            TextColor = Color.FromArgb("#607D8B"),
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };

        var header = new Grid
        {
            Padding = new Thickness(20, 24, 20, 16),
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb("#0D47A1"), 0),
                    new GradientStop(Color.FromArgb("#1565C0"), 0.5f),
                    new GradientStop(Color.FromArgb("#1E88E5"), 1)
                }
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(52),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 12
        };

        var badgeBorder = new Border
        {
            WidthRequest = 48,
            HeightRequest = 48,
            StrokeThickness = 0,
            BackgroundColor = Color.FromRgba(255, 255, 255, 0.2),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Content = new Label
            {
                Text = badge,
                FontFamily = "OpenSansSemibold",
                FontSize = 18,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var titleStack = new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label
                {
                    Text = headline,
                    FontFamily = "OpenSansSemibold",
                    FontSize = 17,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text = tagline,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#E3F2FD"),
                    LineBreakMode = LineBreakMode.WordWrap,
                    MaxLines = 2
                }
            }
        };

        header.Add(badgeBorder, 0, 0);
        header.Add(titleStack, 1, 0);

        var flyoutRoot = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 0,
                Children =
                {
                    header,
                    new BoxView { HeightRequest = 1, Color = Color.FromArgb("#C5D9E8") },
                    menuStack,
                    new BoxView { HeightRequest = 1, Color = Color.FromArgb("#C5D9E8"), Margin = new Thickness(16, 12) },
                    versionLabel,
                    logoutButton
                }
            }
        };

        return new ContentPage
        {
            Title = "Menü",
            BackgroundColor = Color.FromArgb("#FAFCFE"),
            Content = flyoutRoot,
            IconImageSource = null
        };
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var store = AppLocator.Services.GetRequiredService<IAuthTokenStore>();
        store.Clear();
        Current = null;
        PortalRouteTable.Clear();
        await AppNavigator.ReturnToLoginAsync().ConfigureAwait(false);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (ReferenceEquals(Current, this))
        {
            Current = null;
        }
    }
}
