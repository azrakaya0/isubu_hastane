namespace Hospital.Client;

/// <summary>
/// Shell içinde FlyoutItem / ShellContent rotasına geçiş. PushAsync yerine kullanılır;
/// aksi halde sekme ile tutarsız yığın oluşabiliyor ve Windows’ta sayfa açılmıyor gibi görünebiliyor.
/// </summary>
internal static class ShellFlyoutNavigator
{
    internal static Task GoToAsync(string shellContentRoute)
    {
        if (Shell.Current is null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current.GoToAsync("//" + shellContentRoute);
    }
}
