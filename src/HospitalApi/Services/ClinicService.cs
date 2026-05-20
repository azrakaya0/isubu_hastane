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
            linq = linq.Where(c =>
                c.Name.Contains(term) ||
                (c.Description != null && c.Description.Contains(term)) ||
                (c.ClinicNumber != null && c.ClinicNumber.Contains(term)));
        }

        return await linq
            .OrderBy(c => c.ClinicNumber ?? "zzz")
            .ThenBy(c => c.Name)
            .Select(c => Map(c))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await db.Clinics.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => Map(c))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ClinicDto> CreateAsync(CreateClinicRequest request, CancellationToken cancellationToken = default)
    {
        var numberError = await ValidateClinicNumberAsync(request.ClinicNumber, excludeId: null, cancellationToken);
        if (numberError is not null)
        {
            throw new InvalidOperationException(numberError);
        }

        var entity = new Clinic
        {
            ClinicNumber = NormalizeClinicNumber(request.ClinicNumber),
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

        var numberError = await ValidateClinicNumberAsync(request.ClinicNumber, excludeId: id, cancellationToken);
        if (numberError is not null)
        {
            return (false, numberError);
        }

        entity.ClinicNumber = NormalizeClinicNumber(request.ClinicNumber);
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

    private static ClinicDto Map(Clinic c) =>
        new()
        {
            Id = c.Id,
            ClinicNumber = c.ClinicNumber,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            CreatedByUserId = c.CreatedByUserId,
            UpdatedAt = c.UpdatedAt,
            UpdatedByUserId = c.UpdatedByUserId
        };

    private static string? NormalizeClinicNumber(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string?> ValidateClinicNumberAsync(string? clinicNumber, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = NormalizeClinicNumber(clinicNumber);
        if (normalized is null)
        {
            return null;
        }

        if (normalized.Length > 20)
        {
            return "Poliklinik numarası en fazla 20 karakter olabilir.";
        }

        var exists = await db.Clinics.AsNoTracking()
            .AnyAsync(c => c.ClinicNumber == normalized && (excludeId == null || c.Id != excludeId), cancellationToken);
        return exists ? "Bu poliklinik numarası zaten kullanılıyor." : null;
    }
}
