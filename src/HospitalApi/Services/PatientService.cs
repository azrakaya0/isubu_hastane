using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class PatientService(HospitalDbContext db, ICurrentUserContext currentUser) : IPatientService
{
    public async Task<IReadOnlyList<PatientDto>> GetAsync(PatientListQuery query, CancellationToken cancellationToken = default)
    {
        var linq = db.Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            linq = linq.Where(p =>
                p.FirstName.Contains(term) ||
                p.LastName.Contains(term) ||
                (p.Email != null && p.Email.Contains(term)) ||
                p.Phone.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.NationalId))
        {
            var nid = query.NationalId.Trim();
            linq = linq.Where(p => p.NationalId == nid);
        }

        return await linq
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                NationalId = p.NationalId,
                Phone = p.Phone,
                Email = p.Email,
                BirthDate = p.BirthDate,
                CreatedAt = p.CreatedAt,
                CreatedByUserId = p.CreatedByUserId,
                UpdatedAt = p.UpdatedAt,
                UpdatedByUserId = p.UpdatedByUserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Patients.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatientDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                NationalId = p.NationalId,
                Phone = p.Phone,
                Email = p.Email,
                BirthDate = p.BirthDate,
                CreatedAt = p.CreatedAt,
                CreatedByUserId = p.CreatedByUserId,
                UpdatedAt = p.UpdatedAt,
                UpdatedByUserId = p.UpdatedByUserId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, PatientDto? Patient)> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await db.Patients.AnyAsync(p => p.NationalId == request.NationalId, cancellationToken);
        if (exists)
        {
            return (false, "Bu T.C. kimlik numarası ile kayıtlı hasta zaten var.", null);
        }

        var userId = currentUser.UserId;
        var entity = new Patient
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            NationalId = request.NationalId.Trim(),
            Phone = request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            BirthDate = request.BirthDate.Date,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        db.Patients.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var dto = await GetByIdAsync(entity.Id, cancellationToken);
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Hasta bulunamadı.");
        }

        var duplicate = await db.Patients.AnyAsync(
            p => p.NationalId == request.NationalId && p.Id != id,
            cancellationToken);
        if (duplicate)
        {
            return (false, "Bu T.C. kimlik numarası başka bir hastaya ait.");
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.NationalId = request.NationalId.Trim();
        entity.Phone = request.Phone.Trim();
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        entity.BirthDate = request.BirthDate.Date;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUser.UserId;

        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Patients.Include(p => p.Appointments).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Hasta bulunamadı.");
        }

        if (entity.Appointments.Any())
        {
            return (false, "Randevusu olan hasta silinemez.");
        }

        db.Patients.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
