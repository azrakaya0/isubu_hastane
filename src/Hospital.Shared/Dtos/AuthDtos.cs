using System.ComponentModel.DataAnnotations;

namespace Hospital.Shared.Dtos;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>Admin, Doctor veya Patient.</summary>
    public string Role { get; set; } = "Admin";
}

public sealed class DoctorPortalLoginRequest
{
    [Required(ErrorMessage = "Portal kullanıcı adı zorunludur.")]
    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public sealed class PatientPortalLoginRequest
{
    [Required(ErrorMessage = "T.C. kimlik numarası zorunludur.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. kimlik numarası 11 haneli olmalıdır.")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
    [MaxLength(128)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
