using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hospital.Client.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();
		RequestedTheme = ApplicationTheme.Light;
		UnhandledException += OnUnhandledException;
	}

	private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		e.Handled = true;
		var path = System.IO.Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "maui_crash.txt");
		System.IO.File.AppendAllText(path, $"\n[{DateTime.Now}] WinUI: {e.Exception}\n{e.Exception?.StackTrace}\n");
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
