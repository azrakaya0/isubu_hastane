using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

internal static class PatientBookingHelper
{
    internal static async Task<(bool Success, string? Error, int PatientId)> ResolvePatientIdAsync(
        HospitalDbContext db,
        IPasswordHasher<Patient> patientPasswordHasher,
        int? currentUserId,
        int existingPatientId,
        AppointmentWalkInPatientRequest? walkIn,
        CancellationToken cancellationToken)
    {
        if (existingPatientId > 0)
        {
            if (!await db.Patients.AnyAsync(p => p.Id == existingPatientId, cancellationToken))
            {
                return (false, "Hasta bulunamadı.", 0);
            }

            return (true, null, existingPatientId);
        }

        if (walkIn is null)
        {
            return (false, "Hasta seçin veya yeni hasta bilgisi girin.", 0);
        }

        var first = walkIn.FirstName?.Trim() ?? string.Empty;
        var last = walkIn.LastName?.Trim() ?? string.Empty;
        var nid = walkIn.NationalId?.Trim();

        if (!string.IsNullOrEmpty(nid))
        {
            if (nid.Length != 11 || !nid.All(char.IsDigit))
            {
                return (false, "T.C. kimlik numarası 11 haneli olmalıdır.", 0);
            }

            var existing = await db.Patients.FirstOrDefaultAsync(p => p.NationalId == nid, cancellationToken);
            if (existing is not null)
            {
                return (true, null, existing.Id);
            }
        }

        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
        {
            return (false, "Kayıtlı olmayan hasta için ad ve soyad giriniz.", 0);
        }

        if (string.IsNullOrEmpty(nid))
        {
            nid = await GenerateWalkInNationalIdAsync(db, cancellationToken);
        }

        var phone = string.IsNullOrWhiteSpace(walkIn.Phone) ? "0000000000" : walkIn.Phone.Trim();
        var birth = walkIn.BirthDate?.Date ?? DateTime.Today.AddYears(-30);

        var entity = new Patient
        {
            FirstName = first,
            LastName = last,
            NationalId = nid,
            Phone = phone,
            BirthDate = birth,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUserId
        };
        entity.PortalPasswordHash = patientPasswordHasher.HashPassword(entity, "Patient123!");
        db.Patients.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null, entity.Id);
    }

    private static async Task<string> GenerateWalkInNationalIdAsync(HospitalDbContext db, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 50; i++)
        {
            var candidate = "9" + Random.Shared.Next(100000000, 999999999).ToString();
            if (!await db.Patients.AnyAsync(p => p.NationalId == candidate, cancellationToken))
            {
                return candidate;
            }
        }

        return "9" + DateTime.UtcNow.Ticks.ToString()[^10..];
    }
}
