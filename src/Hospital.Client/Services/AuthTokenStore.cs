namespace Hospital.Client.Services;

public sealed class AuthTokenStore : IAuthTokenStore
{
    private const string TokenKey = "hospital_jwt";
    private const string RoleKey = "hospital_role";
    private const string DisplayNameKey = "hospital_display_name";

    public string? Token => Preferences.Get(TokenKey, string.Empty) is { Length: > 0 } t ? t : null;

    public string? Role => Preferences.Get(RoleKey, string.Empty) is { Length: > 0 } r ? r : null;

    public string? DisplayName => Preferences.Get(DisplayNameKey, string.Empty) is { Length: > 0 } n ? n : null;

    public void SetSession(string token, string role, string? displayName = null)
    {
        Preferences.Set(TokenKey, token);
        Preferences.Set(RoleKey, role);
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            Preferences.Set(DisplayNameKey, displayName);
        }
    }

    public void SetToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            Clear();
        }
        else
        {
            SetSession(token, "Admin");
        }
    }

    public void Clear()
    {
        Preferences.Remove(TokenKey);
        Preferences.Remove(RoleKey);
        Preferences.Remove(DisplayNameKey);
    }
}
