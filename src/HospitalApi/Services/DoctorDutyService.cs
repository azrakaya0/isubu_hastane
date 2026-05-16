using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class DoctorDutyService(HospitalDbContext db, ICurrentUserContext currentUser) : IDoctorDutyService
{
    public async Task<IReadOnlyList<DoctorDutyDto>> GetAsync(DoctorDutyListQuery query, CancellationToken cancellationToken = default)
    {
        var linq = from d in db.DoctorDuties.AsNoTracking()
                   join doc in db.Doctors.AsNoTracking() on d.DoctorId equals doc.Id
                   join c in db.Clinics.AsNoTracking() on d.ClinicId equals c.Id
                   select new { Duty = d, Doctor = doc, Clinic = c };

        if (query.DoctorId is int did && did > 0)
        {
            linq = linq.Where(x => x.Duty.DoctorId == did);
        }

        if (query.ClinicId is int cid && cid > 0)
        {
            linq = linq.Where(x => x.Duty.ClinicId == cid);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value.Date;
            linq = linq.Where(x => x.Duty.DutyDate >= from);
        }

        if (query.To.HasValue)
        {
            var to = query.To.Value.Date;
            linq = linq.Where(x => x.Duty.DutyDate <= to);
        }

        return await linq
            .OrderByDescending(x => x.Duty.DutyDate)
            .ThenBy(x => x.Doctor.LastName)
            .Select(x => new DoctorDutyDto
            {
                Id = x.Duty.Id,
                DoctorId = x.Duty.DoctorId,
                DoctorFullName = x.Doctor.FirstName + " " + x.Doctor.LastName,
                ClinicId = x.Duty.ClinicId,
                ClinicName = x.Clinic.Name,
                DutyDate = x.Duty.DutyDate,
                DutyKind = x.Duty.DutyKind,
                Notes = x.Duty.Notes,
                CreatedAt = x.Duty.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DoctorDutyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await (from d in db.DoctorDuties.AsNoTracking()
                      join doc in db.Doctors.AsNoTracking() on d.DoctorId equals doc.Id
                      join c in db.Clinics.AsNoTracking() on d.ClinicId equals c.Id
                      where d.Id == id
                      select new DoctorDutyDto
                      {
                          Id = d.Id,
                          DoctorId = d.DoctorId,
                          DoctorFullName = doc.FirstName + " " + doc.LastName,
                          ClinicId = d.ClinicId,
                          ClinicName = c.Name,
                          DutyDate = d.DutyDate,
                          DutyKind = d.DutyKind,
                          Notes = d.Notes,
                          CreatedAt = d.CreatedAt
                      }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, DoctorDutyDto? Duty)> CreateAsync(CreateDoctorDutyRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.Doctors.AnyAsync(d => d.Id == request.DoctorId, cancellationToken))
        {
            return (false, "Doktor bulunamadı.", null);
        }

        if (!await db.Clinics.AnyAsync(c => c.Id == request.ClinicId, cancellationToken))
        {
            return (false, "Poliklinik bulunamadı.", null);
        }

        var doctor = await db.Doctors.AsNoTracking().FirstAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (doctor.ClinicId != request.ClinicId)
        {
            return (false, "Seçilen doktor bu poliklinikte görevli değil.", null);
        }

        var entity = new DoctorDuty
        {
            DoctorId = request.DoctorId,
            ClinicId = request.ClinicId,
            DutyDate = request.DutyDate.Date,
            DutyKind = request.DutyKind.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };
        db.DoctorDuties.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null, await GetByIdAsync(entity.Id, cancellationToken));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDoctorDutyRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.DoctorDuties.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Kayıt bulunamadı.");
        }

        if (!await db.Doctors.AnyAsync(d => d.Id == request.DoctorId, cancellationToken))
        {
            return (false, "Doktor bulunamadı.");
        }

        if (!await db.Clinics.AnyAsync(c => c.Id == request.ClinicId, cancellationToken))
        {
            return (false, "Poliklinik bulunamadı.");
        }

        var doctor = await db.Doctors.AsNoTracking().FirstAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (doctor.ClinicId != request.ClinicId)
        {
            return (false, "Seçilen doktor bu poliklinikte görevli değil.");
        }

        entity.DoctorId = request.DoctorId;
        entity.ClinicId = request.ClinicId;
        entity.DutyDate = request.DutyDate.Date;
        entity.DutyKind = request.DutyKind.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUser.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.DoctorDuties.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Kayıt bulunamadı.");
        }

        db.DoctorDuties.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
