using Hospital.Shared.Enums;

namespace HospitalApi.Entities;

public sealed class Appointment : AuditableEntity
{
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public int ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    public DateTime ScheduledAt { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
}
