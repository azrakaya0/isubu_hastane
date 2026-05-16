using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface ILabReportService
{
    Task<IReadOnlyList<LabReportDto>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LabReportDto>> GetByPatientForDoctorAsync(int patientId, int doctorId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LabReportDto>> GetAllAsync(int? patientId, CancellationToken cancellationToken = default);

    Task<LabReportDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, LabReportDto? Report)> CreateAsync(CreateLabReportRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateLabReportRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
