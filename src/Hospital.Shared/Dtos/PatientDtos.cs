using System.ComponentModel.DataAnnotations;

namespace Hospital.Shared.Dtos;

public sealed class PatientDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

public class CreatePatientRequest
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "T.C. kimlik numarası zorunludur.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. kimlik 11 haneli olmalıdır.")]
    public string NationalId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [MaxLength(200)]
    public string? Email { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }
}

public sealed class UpdatePatientRequest : CreatePatientRequest
{
}

public sealed class PatientListQuery
{
    public string? Search { get; set; }
    public string? NationalId { get; set; }
}
