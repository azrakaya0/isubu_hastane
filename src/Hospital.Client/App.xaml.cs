namespace Hospital.Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        MainPage = CreateLoginNavigation();
    }

    /// <summary>
    /// Giriş sayfası NavigationPage içinde olduğundan üst çubuk varsayılan olarak görünür;
    /// kapatılmazsa Windows'ta koyu başlık ve sistem vurgu renkleri hissedilir.
    /// </summary>
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
