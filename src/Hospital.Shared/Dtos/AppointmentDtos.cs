using System.ComponentModel.DataAnnotations;
using Hospital.Shared.Enums;

namespace Hospital.Shared.Dtos;

public sealed class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;
    public int ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}

public sealed class CreateAppointmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Hasta seçiniz.")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Doktor seçiniz.")]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Poliklinik seçiniz.")]
    public int ClinicId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class UpdateAppointmentRequest
{
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue)]
    public int ClinicId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    public AppointmentStatus Status { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class AppointmentListQuery
{
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }
    public int? ClinicId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public AppointmentStatus? Status { get; set; }
}
