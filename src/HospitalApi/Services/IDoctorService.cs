using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorDto>> GetAsync(DoctorListQuery query, CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, DoctorDto? Doctor)> CreateAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
