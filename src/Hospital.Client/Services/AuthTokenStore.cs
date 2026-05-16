namespace Hospital.Client.Services;

public sealed class AuthTokenStore : IAuthTokenStore
{
    private const string TokenKey = "hospital_jwt";
    private const string RoleKey = "hospital_role";

    public string? Token => Preferences.Get(TokenKey, string.Empty) is { Length: > 0 } t ? t : null;

    public string? Role => Preferences.Get(RoleKey, string.Empty) is { Length: > 0 } r ? r : null;

    public void SetSession(string token, string role)
    {
        Preferences.Set(TokenKey, token);
        Preferences.Set(RoleKey, role);
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
    }
}
