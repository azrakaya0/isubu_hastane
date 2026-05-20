using Hospital.Client.Portal;



namespace Hospital.Client;



/// <summary>

/// Giriş / çıkış sonrası güvenli sayfa geçişi.

/// </summary>

internal static class AppNavigator

{

    internal static Task EnterAdminPortalAsync() =>

        EnterPortalAsync(PortalKind.Admin);



    internal static Task EnterDoctorPortalAsync() =>

        EnterPortalAsync(PortalKind.Doctor);



    internal static Task EnterPatientPortalAsync() =>

        EnterPortalAsync(PortalKind.Patient);

    internal static Task EnterPortalAfterLoginAsync(LoginPortalKind portalKind) =>
        EnterPortalAsync(portalKind switch
        {
            LoginPortalKind.Doctor => PortalKind.Doctor,
            LoginPortalKind.Patient => PortalKind.Patient,
            _ => PortalKind.Admin
        });

    private static Task EnterPortalAsync(PortalKind kind)
    {
        CrashLogger.Log($"EnterPortal {kind}", "AppNavigator");

        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            try
            {
                if (Application.Current is not { } app)
                {
                    return;
                }

                PortalFlyoutHost.Current = null;
                PortalRouteTable.Clear();

                if (PortalWindowsNavigation.UseNavigationPage)
                {
                    CrashLogger.Log($"Windows bootstrap portal ({kind})", "AppNavigator");
                    app.MainPage = PortalWindowsNavigation.CreateBootstrap(kind);
                }
                else
                {
                    CrashLogger.Log($"Shell portal ({kind})", "AppNavigator");
                    app.MainPage = kind switch
                    {
                        PortalKind.Doctor => new DoctorShell(),
                        PortalKind.Patient => new PatientShell(),
                        _ => new AppShell()
                    };
                }

                CrashLogger.Log($"MainPage={app.MainPage?.GetType().Name}", "AppNavigator");
            }
            catch (Exception ex)
            {
                CrashLogger.Log(ex, "AppNavigator.EnterPortalAsync");
                System.Diagnostics.Debug.WriteLine($"Portal girişi hatası: {ex}");
                _ = ReturnToLoginAsync();
            }
        });
    }



    internal static Task ReturnToLoginAsync()

    {

        PortalFlyoutHost.Current = null;

        PortalRouteTable.Clear();



        return MainThread.InvokeOnMainThreadAsync(() =>

        {

            if (Application.Current is not { } app)

            {

                return;

            }



            app.MainPage = App.CreateLoginNavigation();

        });

    }

}


