namespace HospitalApi.Entities;

/// <summary>Doktor nöbet, izin veya poliklinik mesai kaydı.</summary>
public sealed class DoctorDuty : AuditableEntity
{
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public int ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public DateTime DutyDate { get; set; }

    /// <summary>Nöbet, İzin, Poliklinik mesaisi</summary>
    public string DutyKind { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
