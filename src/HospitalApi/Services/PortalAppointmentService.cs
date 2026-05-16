using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;
using HospitalApi.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class PortalAppointmentService(
    HospitalDbContext db,
    IAppointmentService appointments) : IPortalAppointmentService
{
    public async Task<(bool Success, string? Error, AppointmentDto? Appointment)> PatientBookAsync(
        int patientId,
        PortalPatientBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var create = new CreateAppointmentRequest
        {
            PatientId = patientId,
            DoctorId = request.DoctorId,
            ClinicId = request.ClinicId,
            ScheduledAt = request.ScheduledAt,
            Status = AppointmentStatus.Scheduled,
            Notes = request.Notes
        };

        return await appointments.CreateAsync(create, cancellationToken);
    }

    public async Task<(bool Success, string? Error, AppointmentDto? Appointment)> DoctorBookAsync(
        int doctorId,
        PortalDoctorBookRequest request,
        CancellationToken cancellationToken = default)
    {
        var nationalId = request.PatientNationalId.Trim();
        var patient = await db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.NationalId == nationalId, cancellationToken);
        if (patient is null)
        {
            return (false, "Bu T.C. kimlik numarası ile kayıtlı hasta bulunamadı.", null);
        }

        if (doctorId <= 0)
        {
            return (false, "Doktor bilgisi geçersiz.", null);
        }

        var create = new CreateAppointmentRequest
        {
            PatientId = patient.Id,
            DoctorId = doctorId,
            ClinicId = request.ClinicId,
            ScheduledAt = request.ScheduledAt,
            Status = request.Status,
            Notes = request.Notes
        };

        return await appointments.CreateAsync(create, cancellationToken);
    }
}
