using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class AppointmentService(HospitalDbContext db, ICurrentUserContext currentUser) : IAppointmentService
{
    public async Task<IReadOnlyList<AppointmentDto>> GetAsync(AppointmentListQuery query, CancellationToken cancellationToken = default)
    {
        var linq = from a in db.Appointments.AsNoTracking()
                   join p in db.Patients.AsNoTracking() on a.PatientId equals p.Id
                   join d in db.Doctors.AsNoTracking() on a.DoctorId equals d.Id
                   join c in db.Clinics.AsNoTracking() on a.ClinicId equals c.Id
                   select new { Appointment = a, Patient = p, Doctor = d, Clinic = c };

        if (query.PatientId is int pid && pid > 0)
        {
            linq = linq.Where(x => x.Appointment.PatientId == pid);
        }

        if (query.DoctorId is int did && did > 0)
        {
            linq = linq.Where(x => x.Appointment.DoctorId == did);
        }

        if (query.ClinicId is int cid && cid > 0)
        {
            linq = linq.Where(x => x.Appointment.ClinicId == cid);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value;
            linq = linq.Where(x => x.Appointment.ScheduledAt >= from);
        }

        if (query.To.HasValue)
        {
            var to = query.To.Value;
            linq = linq.Where(x => x.Appointment.ScheduledAt <= to);
        }

        if (query.Status.HasValue)
        {
            var status = query.Status.Value;
            linq = linq.Where(x => x.Appointment.Status == status);
        }

        return await linq
            .OrderByDescending(x => x.Appointment.ScheduledAt)
            .Select(x => new AppointmentDto
            {
                Id = x.Appointment.Id,
                PatientId = x.Appointment.PatientId,
                PatientFullName = x.Patient.FirstName + " " + x.Patient.LastName,
                DoctorId = x.Appointment.DoctorId,
                DoctorFullName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                ClinicId = x.Appointment.ClinicId,
                ClinicName = x.Clinic.Name,
                ScheduledAt = x.Appointment.ScheduledAt,
                Status = x.Appointment.Status,
                Notes = x.Appointment.Notes,
                CreatedAt = x.Appointment.CreatedAt,
                CreatedByUserId = x.Appointment.CreatedByUserId,
                UpdatedAt = x.Appointment.UpdatedAt,
                UpdatedByUserId = x.Appointment.UpdatedByUserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await (from a in db.Appointments.AsNoTracking()
                      join p in db.Patients.AsNoTracking() on a.PatientId equals p.Id
                      join d in db.Doctors.AsNoTracking() on a.DoctorId equals d.Id
                      join c in db.Clinics.AsNoTracking() on a.ClinicId equals c.Id
                      where a.Id == id
                      select new AppointmentDto
                      {
                          Id = a.Id,
                          PatientId = a.PatientId,
                          PatientFullName = p.FirstName + " " + p.LastName,
                          DoctorId = a.DoctorId,
                          DoctorFullName = d.FirstName + " " + d.LastName,
                          ClinicId = a.ClinicId,
                          ClinicName = c.Name,
                          ScheduledAt = a.ScheduledAt,
                          Status = a.Status,
                          Notes = a.Notes,
                          CreatedAt = a.CreatedAt,
                          CreatedByUserId = a.CreatedByUserId,
                          UpdatedAt = a.UpdatedAt,
                          UpdatedByUserId = a.UpdatedByUserId
                      }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, AppointmentDto? Appointment)> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var patientExists = await db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken);
        if (!patientExists)
        {
            return (false, "Hasta bulunamadı.", null);
        }

        var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return (false, "Doktor bulunamadı.", null);
        }

        var clinicExists = await db.Clinics.AnyAsync(c => c.Id == request.ClinicId, cancellationToken);
        if (!clinicExists)
        {
            return (false, "Poliklinik bulunamadı.", null);
        }

        if (doctor.ClinicId != request.ClinicId)
        {
            return (false, "Seçilen doktor bu poliklinikte görevli değil.", null);
        }

        var entity = new Appointment
        {
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            ClinicId = request.ClinicId,
            ScheduledAt = request.ScheduledAt,
            Status = request.Status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };

        db.Appointments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(entity.Id, cancellationToken);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Randevu bulunamadı.");
        }

        var patientExists = await db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken);
        if (!patientExists)
        {
            return (false, "Hasta bulunamadı.");
        }

        var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (doctor is null)
        {
            return (false, "Doktor bulunamadı.");
        }

        var clinicExists = await db.Clinics.AnyAsync(c => c.Id == request.ClinicId, cancellationToken);
        if (!clinicExists)
        {
            return (false, "Poliklinik bulunamadı.");
        }

        if (doctor.ClinicId != request.ClinicId)
        {
            return (false, "Seçilen doktor bu poliklinikte görevli değil.");
        }

        entity.PatientId = request.PatientId;
        entity.DoctorId = request.DoctorId;
        entity.ClinicId = request.ClinicId;
        entity.ScheduledAt = request.ScheduledAt;
        entity.Status = request.Status;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Randevu bulunamadı.");
        }

        db.Appointments.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
