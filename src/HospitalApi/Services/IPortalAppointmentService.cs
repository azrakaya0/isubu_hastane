using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IPortalAppointmentService
{
    Task<(bool Success, string? Error, AppointmentDto? Appointment)> PatientBookAsync(int patientId, PortalPatientBookRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, AppointmentDto? Appointment)> DoctorBookAsync(int doctorId, PortalDoctorBookRequest request, CancellationToken cancellationToken = default);
}
