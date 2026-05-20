using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IPatientService
{
    Task<IReadOnlyList<PatientDto>> GetAsync(PatientListQuery query, CancellationToken cancellationToken = default);
    Task<PatientDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, PatientDto? Patient)> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, PatientDto? Patient)> UpdatePortalProfileAsync(
        int patientId,
        UpdatePatientProfileRequest request,
        CancellationToken cancellationToken = default);
}
