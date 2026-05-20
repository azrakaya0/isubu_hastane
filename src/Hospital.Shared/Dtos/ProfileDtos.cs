using System.ComponentModel.DataAnnotations;

namespace Hospital.Shared.Dtos;

public sealed class UpdatePatientProfileRequest
{
    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    [MaxLength(80)]
    public string? EmergencyContactRelation { get; set; }
}

public sealed class HospitalContactInfo
{
    public string HospitalName { get; set; } = "İSUBÜ Hastanesi · Merkez";
    public string Department { get; set; } = "İSUBÜ Sağlık Hizmetleri Birim Yönetimi";
    public string Phone { get; set; } = "+90 (246) 211 00 00";
    public string EmergencyLine { get; set; } = "112";
    public string Email { get; set; } = "iletisim@isubu-hastane.tr";
    public string Hours { get; set; } = "Poliklinik: hafta içi 08:00–17:00 · Acil servis: 7/24";
    public string Address { get; set; } = "Kampüs sağlık merkezi, zemin kat danışma";
}
