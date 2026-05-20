using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class DoctorService(HospitalDbContext db, ICurrentUserContext currentUser) : IDoctorService
{
    public async Task<IReadOnlyList<DoctorDto>> GetAsync(DoctorListQuery query, CancellationToken cancellationToken = default)
    {
        var linq = from d in db.Doctors.AsNoTracking()
                   join c in db.Clinics.AsNoTracking() on d.ClinicId equals c.Id
                   select new { Doctor = d, ClinicName = c.Name, ClinicNumber = c.ClinicNumber };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            linq = linq.Where(x =>
                x.Doctor.FirstName.Contains(term) ||
                x.Doctor.LastName.Contains(term) ||
                x.Doctor.Specialty.Contains(term) ||
                x.ClinicName.Contains(term));
        }

        if (query.ClinicId is int clinicId && clinicId > 0)
        {
            linq = linq.Where(x => x.Doctor.ClinicId == clinicId);
        }

        return await linq
            .OrderBy(x => x.Doctor.LastName)
            .ThenBy(x => x.Doctor.FirstName)
            .Select(x => new DoctorDto
            {
                Id = x.Doctor.Id,
                FirstName = x.Doctor.FirstName,
                LastName = x.Doctor.LastName,
                Specialty = x.Doctor.Specialty,
                ClinicId = x.Doctor.ClinicId,
                ClinicName = x.ClinicName,
                ClinicNumber = x.ClinicNumber,
                OfficeLocation = x.Doctor.OfficeLocation,
                PublicPhone = x.Doctor.PublicPhone,
                PublicEmail = x.Doctor.PublicEmail,
                CreatedAt = x.Doctor.CreatedAt,
                CreatedByUserId = x.Doctor.CreatedByUserId,
                UpdatedAt = x.Doctor.UpdatedAt,
                UpdatedByUserId = x.Doctor.UpdatedByUserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await (from d in db.Doctors.AsNoTracking()
                      join c in db.Clinics.AsNoTracking() on d.ClinicId equals c.Id
                      where d.Id == id
                      select new DoctorDto
                      {
                          Id = d.Id,
                          FirstName = d.FirstName,
                          LastName = d.LastName,
                          Specialty = d.Specialty,
                          ClinicId = d.ClinicId,
                          ClinicName = c.Name,
                          ClinicNumber = c.ClinicNumber,
                          OfficeLocation = d.OfficeLocation,
                          PublicPhone = d.PublicPhone,
                          PublicEmail = d.PublicEmail,
                          CreatedAt = d.CreatedAt,
                          CreatedByUserId = d.CreatedByUserId,
                          UpdatedAt = d.UpdatedAt,
                          UpdatedByUserId = d.UpdatedByUserId
                      }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, DoctorDto? Doctor)> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var clinicExists = await db.Clinics.AnyAsync(c => c.Id == request.ClinicId, cancellationToken);
        if (!clinicExists)
        {
            return (false, "Poliklinik bulunamadı.", null);
        }

        var entity = new Doctor
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Specialty = request.Specialty.Trim(),
            ClinicId = request.ClinicId,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };

        db.Doctors.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var dto = await GetByIdAsync(entity.Id, cancellationToken);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Doktor bulunamadı.");
        }

        var clinicExists = await db.Clinics.AnyAsync(c => c.Id == request.ClinicId, cancellationToken);
        if (!clinicExists)
        {
            return (false, "Poliklinik bulunamadı.");
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Specialty = request.Specialty.Trim();
        entity.ClinicId = request.ClinicId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUser.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Doctors.Include(d => d.Appointments).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Doktor bulunamadı.");
        }

        if (entity.Appointments.Any())
        {
            return (false, "Randevusu olan doktor silinemez.");
        }

        db.Doctors.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
