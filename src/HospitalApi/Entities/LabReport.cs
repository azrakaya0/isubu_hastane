namespace HospitalApi.Entities;

/// <summary>Hasta için laboratuvar / tetkik özeti (tek hastane portalı).</summary>
public sealed class LabReport : AuditableEntity
{
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Örn: Kan, İdrar, Radyoloji, Patoloji</summary>
    public string Category { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime ResultDate { get; set; }

    public int? OrderingDoctorId { get; set; }
    public Doctor? OrderingDoctor { get; set; }

    public string? PdfFileName { get; set; }
}
