namespace HospitalApi.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "HospitalApi";
    public string Audience { get; set; } = "HospitalClient";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 120;
}
