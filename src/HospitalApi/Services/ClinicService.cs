using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class ClinicService(HospitalDbContext db, ICurrentUserContext currentUser) : IClinicService
{
    public async Task<IReadOnlyList<ClinicDto>> GetAsync(ClinicListQuery query, CancellationToken cancellationToken = default)
    {
        var linq = db.Clinics.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            linq = linq.Where(c => c.Name.Contains(term) || (c.Description != null && c.Description.Contains(term)));
        }

        return await linq
            .OrderBy(c => c.Name)
            .Select(c => new ClinicDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                CreatedByUserId = c.CreatedByUserId,
                UpdatedAt = c.UpdatedAt,
                UpdatedByUserId = c.UpdatedByUserId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Clinics.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClinicDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                CreatedByUserId = c.CreatedByUserId,
                UpdatedAt = c.UpdatedAt,
                UpdatedByUserId = c.UpdatedByUserId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClinicDto> CreateAsync(CreateClinicRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Clinic
        {
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };
        db.Clinics.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateClinicRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.Clinics.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Poliklinik bulunamadı.");
        }

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUser.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Clinics
            .Include(c => c.Doctors)
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Poliklinik bulunamadı.");
        }

        if (entity.Doctors.Any() || entity.Appointments.Any())
        {
            return (false, "Doktoru veya randevusu olan poliklinik silinemez.");
        }

        db.Clinics.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
