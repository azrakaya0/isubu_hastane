using Hospital.Client.Configuration;
using Hospital.Client.Services;
using Microsoft.Extensions.Logging;

namespace Hospital.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IAuthTokenStore, AuthTokenStore>();
        builder.Services.AddTransient<AuthHeaderHandler>();
        builder.Services.AddHttpClient<IHospitalApiClient, HospitalApiClient>(client =>
            {
                client.BaseAddress = new Uri(ApiSettings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        AppLocator.Services = app.Services;
        return app;
    }
}
