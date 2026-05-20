using System.ComponentModel.DataAnnotations;

namespace Hospital.Shared.Dtos;

/// <summary>
/// Randevu oluştururken kayıtlı hasta yoksa kısa bilgiyle hasta açılır veya T.C. ile eşleştirilir.
/// </summary>
public sealed class AppointmentWalkInPatientRequest
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>11 hane; boş bırakılırsa geçici kimlik üretilir.</summary>
    [MaxLength(11)]
    public string? NationalId { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? BirthDate { get; set; }
}
