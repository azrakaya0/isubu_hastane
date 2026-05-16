namespace Hospital.Client;

/// <summary>
/// Liste ve form sayfalarında API çağrıları sırasında yükleme göstergesi.
/// </summary>
internal static class PageUi
{
    internal static async Task WithSpinnerAsync(
        VisualElement overlay,
        ActivityIndicator spinner,
        Func<Task> action)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            overlay.IsVisible = true;
            spinner.IsRunning = true;
        });

        try
        {
            await action();
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                spinner.IsRunning = false;
                overlay.IsVisible = false;
            });
        }
    }

    internal static Task HideListErrorAsync(VisualElement banner) =>
        MainThread.InvokeOnMainThreadAsync(() => banner.IsVisible = false);

    internal static Task ShowListErrorAsync(Label messageLabel, VisualElement banner, string message) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            messageLabel.Text = message;
            banner.IsVisible = true;
        });
}
