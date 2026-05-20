namespace HospitalApi.Entities;

public sealed class Patient : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime BirthDate { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    /// <summary>Hasta portal şifresi; boşsa hasta girişi kapalıdır.</summary>
    public string? PortalPasswordHash { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<LabReport> LabReports { get; set; } = new List<LabReport>();
}
