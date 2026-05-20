namespace Hospital.Client.Services;

public interface IAuthTokenStore
{
    string? Token { get; }

    /// <summary>Admin, Doctor veya Patient; oturum yoksa null.</summary>
    string? Role { get; }

    string? DisplayName { get; }

    void SetSession(string token, string role, string? displayName = null);

    /// <summary>Yalnızca jetonu günceller; rolü Admin varsayar (eski çağrılar için).</summary>
    void SetToken(string? token);

    void Clear();
}
