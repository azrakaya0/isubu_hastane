using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IClinicService
{
    Task<IReadOnlyList<ClinicDto>> GetAsync(ClinicListQuery query, CancellationToken cancellationToken = default);
    Task<ClinicDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ClinicDto> CreateAsync(CreateClinicRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateClinicRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
