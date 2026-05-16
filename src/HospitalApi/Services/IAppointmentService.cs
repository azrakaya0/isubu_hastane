using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentDto>> GetAsync(AppointmentListQuery query, CancellationToken cancellationToken = default);
    Task<AppointmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, AppointmentDto? Appointment)> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
