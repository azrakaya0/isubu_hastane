using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IPortalTimelineService
{
    Task<IReadOnlyList<TimelineEntryDto>> GetPatientTimelineAsync(int patientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimelineEntryDto>> GetDoctorTimelineAsync(int doctorId, CancellationToken cancellationToken = default);
}
