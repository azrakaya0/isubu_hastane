using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IDoctorDutyService
{
    Task<IReadOnlyList<DoctorDutyDto>> GetAsync(DoctorDutyListQuery query, CancellationToken cancellationToken = default);

    Task<DoctorDutyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, DoctorDutyDto? Duty)> CreateAsync(CreateDoctorDutyRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDoctorDutyRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
