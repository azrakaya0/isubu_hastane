using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HospitalApi.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HospitalApi.Security;

public sealed class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public string CreateStaffToken(AppUser user) =>
        CreateToken(
            user.Id.ToString(),
            user.UserName,
            user.FullName,
            "Admin");

    public string CreateDoctorToken(Doctor doctor) =>
        CreateToken(
            doctor.Id.ToString(),
            doctor.PortalUserName ?? string.Empty,
            $"{doctor.FirstName} {doctor.LastName}",
            "Doctor");

    public string CreatePatientToken(Patient patient) =>
        CreateToken(
            patient.Id.ToString(),
            patient.NationalId,
            $"{patient.FirstName} {patient.LastName}",
            "Patient");

    private string CreateToken(string subjectId, string userName, string fullName, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subjectId),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.GivenName, fullName),
            new(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
