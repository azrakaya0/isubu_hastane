using System.ComponentModel.DataAnnotations;
using Hospital.Shared.Enums;

namespace Hospital.Shared.Dtos;

public sealed class TimelineEntryDto
{
    public string EntryType { get; set; } = string.Empty;

    public int Id { get; set; }

    public DateTime At { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }
}

public sealed class PortalPatientBookRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Doktor seçiniz.")]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Poliklinik seçiniz.")]
    public int ClinicId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class PortalDoctorBookRequest
{
    /// <summary>11 hane; kayıtlı hasta yoksa <see cref="WalkInPatient"/> ile yeni kayıt açılır.</summary>
    [MaxLength(11)]
    public string? PatientNationalId { get; set; }

    public AppointmentWalkInPatientRequest? WalkInPatient { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Poliklinik seçiniz.")]
    public int ClinicId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class LabReportDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime ResultDate { get; set; }
    public int? OrderingDoctorId { get; set; }
    public string? OrderingDoctorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasPdf { get; set; }
    public string? PdfFileName { get; set; }
}

public class CreateLabReportRequest
{
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public DateTime ResultDate { get; set; }

    public int? OrderingDoctorId { get; set; }

    /// <summary>İsteğe bağlı PDF (base64).</summary>
    public string? PdfBase64 { get; set; }

    [MaxLength(260)]
    public string? PdfFileName { get; set; }
}

public sealed class UpdateLabReportRequest : CreateLabReportRequest
{
}

public sealed class DoctorDutyDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateTime DutyDate { get; set; }
    public string DutyKind { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDoctorDutyRequest
{
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue)]
    public int ClinicId { get; set; }

    [Required]
    public DateTime DutyDate { get; set; }

    [Required]
    [MaxLength(80)]
    public string DutyKind { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class UpdateDoctorDutyRequest : CreateDoctorDutyRequest
{
}

public sealed class DoctorDutyListQuery
{
    public int? DoctorId { get; set; }
    public int? ClinicId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
