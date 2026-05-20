using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class LabReportService(
    HospitalDbContext db,
    ICurrentUserContext currentUser,
    ILabPdfStorage pdfStorage) : ILabReportService
{
    public async Task<IReadOnlyList<LabReportDto>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await QueryDtos().Where(x => x.PatientId == patientId).OrderByDescending(x => x.ResultDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LabReportDto>> GetByPatientForDoctorAsync(int patientId, int doctorId, CancellationToken cancellationToken = default)
    {
        var hasLink = await db.Appointments.AsNoTracking()
            .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorId, cancellationToken);
        if (!hasLink)
        {
            return [];
        }

        return await GetByPatientAsync(patientId, cancellationToken);
    }

    public async Task<IReadOnlyList<LabReportDto>> GetAllAsync(int? patientId, CancellationToken cancellationToken = default)
    {
        var q = QueryDtos();
        if (patientId is int pid && pid > 0)
        {
            q = q.Where(x => x.PatientId == pid);
        }

        return await q.OrderByDescending(x => x.ResultDate).ToListAsync(cancellationToken);
    }

    public async Task<LabReportDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await QueryDtos().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(bool Success, string? Error, LabReportDto? Report)> CreateAsync(CreateLabReportRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken))
        {
            return (false, "Hasta bulunamadı.", null);
        }

        if (request.OrderingDoctorId is int od && !await db.Doctors.AnyAsync(d => d.Id == od, cancellationToken))
        {
            return (false, "İstem yapan doktor bulunamadı.", null);
        }

        var entity = new LabReport
        {
            PatientId = request.PatientId,
            Title = request.Title.Trim(),
            Category = request.Category.Trim(),
            Summary = request.Summary.Trim(),
            ResultDate = request.ResultDate,
            OrderingDoctorId = request.OrderingDoctorId,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };
        if (!string.IsNullOrWhiteSpace(request.PdfBase64))
        {
            try
            {
                var bytes = Convert.FromBase64String(request.PdfBase64.Trim());
                if (bytes.Length > 5 * 1024 * 1024)
                {
                    return (false, "PDF dosyası en fazla 5 MB olabilir.", null);
                }

                entity.PdfFileName = string.IsNullOrWhiteSpace(request.PdfFileName)
                    ? "rapor.pdf"
                    : request.PdfFileName.Trim();
                db.LabReports.Add(entity);
                await db.SaveChangesAsync(cancellationToken);
                await pdfStorage.SaveAsync(entity.Id, bytes, cancellationToken);
            }
            catch (FormatException)
            {
                return (false, "PDF verisi geçersiz.", null);
            }
        }
        else
        {
            db.LabReports.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return (true, null, await GetByIdAsync(entity.Id, cancellationToken));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateLabReportRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await db.LabReports.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Kayıt bulunamadı.");
        }

        if (!await db.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken))
        {
            return (false, "Hasta bulunamadı.");
        }

        if (request.OrderingDoctorId is int od && !await db.Doctors.AnyAsync(d => d.Id == od, cancellationToken))
        {
            return (false, "İstem yapan doktor bulunamadı.");
        }

        entity.PatientId = request.PatientId;
        entity.Title = request.Title.Trim();
        entity.Category = request.Category.Trim();
        entity.Summary = request.Summary.Trim();
        entity.ResultDate = request.ResultDate;
        entity.OrderingDoctorId = request.OrderingDoctorId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserId = currentUser.UserId;

        if (!string.IsNullOrWhiteSpace(request.PdfBase64))
        {
            try
            {
                var bytes = Convert.FromBase64String(request.PdfBase64.Trim());
                if (bytes.Length > 5 * 1024 * 1024)
                {
                    return (false, "PDF dosyası en fazla 5 MB olabilir.");
                }

                entity.PdfFileName = string.IsNullOrWhiteSpace(request.PdfFileName)
                    ? "rapor.pdf"
                    : request.PdfFileName.Trim();
                await db.SaveChangesAsync(cancellationToken);
                await pdfStorage.SaveAsync(id, bytes, cancellationToken);
                return (true, null);
            }
            catch (FormatException)
            {
                return (false, "PDF verisi geçersiz.");
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.LabReports.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return (false, "Kayıt bulunamadı.");
        }

        pdfStorage.Delete(id);
        db.LabReports.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(byte[]? Content, string? FileName)> GetPdfAsync(int id, CancellationToken cancellationToken = default)
    {
        var report = await db.LabReports.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (report is null || string.IsNullOrWhiteSpace(report.PdfFileName))
        {
            return (null, null);
        }

        var bytes = await pdfStorage.TryReadAsync(id, cancellationToken);
        return bytes is null ? (null, null) : (bytes, report.PdfFileName);
    }

    private IQueryable<LabReportDto> QueryDtos()
    {
        return from r in db.LabReports.AsNoTracking()
               join p in db.Patients.AsNoTracking() on r.PatientId equals p.Id
               join od in db.Doctors.AsNoTracking() on r.OrderingDoctorId equals od.Id into odj
               from od in odj.DefaultIfEmpty()
               select new LabReportDto
               {
                   Id = r.Id,
                   PatientId = r.PatientId,
                   PatientFullName = p.FirstName + " " + p.LastName,
                   Title = r.Title,
                   Category = r.Category,
                   Summary = r.Summary,
                   ResultDate = r.ResultDate,
                   OrderingDoctorId = r.OrderingDoctorId,
                   OrderingDoctorName = od != null ? od.FirstName + " " + od.LastName : null,
                   CreatedAt = r.CreatedAt,
                   HasPdf = r.PdfFileName != null,
                   PdfFileName = r.PdfFileName
               };
    }
}
