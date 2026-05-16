namespace HospitalApi.Entities;

public sealed class Doctor : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public int ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    /// <summary>Doktor portal girişi için benzersiz kullanıcı adı; boşsa portal kapalıdır.</summary>
    public string? PortalUserName { get; set; }

    public string? PortalPasswordHash { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<LabReport> OrderedLabReports { get; set; } = new List<LabReport>();

    public ICollection<DoctorDuty> Duties { get; set; } = new List<DoctorDuty>();
}
