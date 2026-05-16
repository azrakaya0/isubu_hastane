using System.ComponentModel.DataAnnotations;

namespace Hospital.Shared.Dtos;

public sealed class DoctorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

public class CreateDoctorRequest
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    [MaxLength(200)]
    public string Specialty { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir poliklinik seçiniz.")]
    public int ClinicId { get; set; }
}

public sealed class UpdateDoctorRequest : CreateDoctorRequest
{
}

public sealed class DoctorListQuery
{
    public string? Search { get; set; }
    public int? ClinicId { get; set; }
}
