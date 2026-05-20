namespace Hospital.Client.Portal;

/// <summary>
/// Windows NavigationPage portalında menüye erişim.
/// </summary>
internal static class PortalNavigationChrome
{
    private const string MenuToolbarText = "Menü";

    internal static void ApplyMenuToolbar(Page page)
    {
        if (DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            return;
        }

        if (page is PortalWindowsMenuPage)
        {
            return;
        }

        if (page.ToolbarItems.Any(t => t.Text == MenuToolbarText))
        {
            return;
        }

        page.ToolbarItems.Add(new ToolbarItem
        {
            Text = MenuToolbarText,
            Order = ToolbarItemOrder.Primary,
            Priority = 0,
            Command = new Command(async () =>
            {
                if (page.Navigation is null)
                {
                    return;
                }

                await page.Navigation.PushAsync(new PortalWindowsMenuPage());
            })
        });
    }
}
