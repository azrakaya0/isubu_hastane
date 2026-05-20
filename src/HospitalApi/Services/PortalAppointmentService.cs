using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class PortalAppointmentService(
    HospitalDbContext db,
    IAppointmentService appointments,
    IPasswordHasher<Patient> patientPasswordHasher,
    ICurrentUserContext currentUser) : IPortalAppointmentService
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
        if (doctorId <= 0)
        {
            return (false, "Doktor bilgisi geçersiz.", null);
        }

        AppointmentWalkInPatientRequest? walkIn = request.WalkInPatient;
        if (walkIn is null && !string.IsNullOrWhiteSpace(request.PatientNationalId))
        {
            walkIn = new AppointmentWalkInPatientRequest
            {
                NationalId = request.PatientNationalId.Trim()
            };
        }

        if (walkIn is null)
        {
            return (false, "Hasta T.C. kimliği veya ad-soyad bilgisi giriniz.", null);
        }

        var (patientOk, patientError, resolvedPatientId) = await PatientBookingHelper.ResolvePatientIdAsync(
            db,
            patientPasswordHasher,
            currentUser.UserId,
            existingPatientId: 0,
            walkIn,
            cancellationToken);
        if (!patientOk)
        {
            return (false, patientError, null);
        }

        var create = new CreateAppointmentRequest
        {
            PatientId = resolvedPatientId,
            DoctorId = doctorId,
            ClinicId = request.ClinicId,
            ScheduledAt = request.ScheduledAt,
            Status = request.Status,
            Notes = request.Notes
        };

        return await appointments.CreateAsync(create, cancellationToken);
    }
}
