namespace Hospital.Client;

public partial class App : Application
{
    private static readonly string CrashLogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "maui_crash.txt");

    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            File.AppendAllText(CrashLogPath, $"\n[{DateTime.Now}] AppDomain: {e.ExceptionObject}\n");
        TaskScheduler.UnobservedTaskException += (_, e) =>
            File.AppendAllText(CrashLogPath, $"\n[{DateTime.Now}] Task: {e.Exception}\n");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(CreateLoginNavigation());
    }

    public static NavigationPage CreateLoginNavigation()
    {
        var root = new PortalPickerPage();
        NavigationPage.SetHasNavigationBar(root, true);
        return new NavigationPage(root)
        {
            BackgroundColor = Color.FromArgb("#F5FAFF"),
            BarBackgroundColor = Color.FromArgb("#E3F2FD"),
            BarTextColor = Color.FromArgb("#0D47A1")
        };
    }
}
