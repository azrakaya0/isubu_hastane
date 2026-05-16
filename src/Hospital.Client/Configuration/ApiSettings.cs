namespace Hospital.Client.Configuration;

public static class ApiSettings
{
    /// <summary>
    /// HTTP profili (dotnet run --launch-profile http) ile aynı port.
    /// Android emülatör: 10.0.2.2 makine localhost'una yönlendirilir.
    /// </summary>
    public static string BaseUrl =>
#if ANDROID
        "http://10.0.2.2:5059/";
#else
        "http://localhost:5059/";
#endif
}
