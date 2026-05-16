using Hospital.Shared.Dtos;
using HospitalApi.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class PortalTimelineService(HospitalDbContext db) : IPortalTimelineService
{
    public async Task<IReadOnlyList<TimelineEntryDto>> GetPatientTimelineAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var appts = await (from a in db.Appointments.AsNoTracking()
                           join d in db.Doctors.AsNoTracking() on a.DoctorId equals d.Id
                           join c in db.Clinics.AsNoTracking() on a.ClinicId equals c.Id
                           where a.PatientId == patientId
                           select new TimelineEntryDto
                           {
                               EntryType = "Randevu",
                               Id = a.Id,
                               At = a.ScheduledAt,
                               Title = d.FirstName + " " + d.LastName + " · " + c.Name,
                               Subtitle = a.Status.ToString() + (a.Notes != null ? " — " + a.Notes : string.Empty)
                           }).ToListAsync(cancellationToken);

        var labs = await (from r in db.LabReports.AsNoTracking()
                          where r.PatientId == patientId
                          select new TimelineEntryDto
                          {
                              EntryType = "Tetkik",
                              Id = r.Id,
                              At = r.ResultDate,
                              Title = r.Category + ": " + r.Title,
                              Subtitle = r.Summary.Length > 160 ? r.Summary.Substring(0, 160) + "…" : r.Summary
                          }).ToListAsync(cancellationToken);

        return appts.Concat(labs).OrderByDescending(x => x.At).ToList();
    }

    public async Task<IReadOnlyList<TimelineEntryDto>> GetDoctorTimelineAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await (from a in db.Appointments.AsNoTracking()
                      join p in db.Patients.AsNoTracking() on a.PatientId equals p.Id
                      join c in db.Clinics.AsNoTracking() on a.ClinicId equals c.Id
                      where a.DoctorId == doctorId
                      select new TimelineEntryDto
                      {
                          EntryType = "Randevu",
                          Id = a.Id,
                          At = a.ScheduledAt,
                          Title = p.FirstName + " " + p.LastName + " · " + c.Name,
                          Subtitle = a.Status.ToString()
                      })
            .OrderByDescending(x => x.At)
            .ToListAsync(cancellationToken);
    }
}
