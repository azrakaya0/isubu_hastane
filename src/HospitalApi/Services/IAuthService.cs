using Hospital.Shared.Dtos;

namespace HospitalApi.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> LoginDoctorPortalAsync(DoctorPortalLoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> LoginPatientPortalAsync(PatientPortalLoginRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> ChangeDoctorPortalPasswordAsync(int doctorId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> ChangePatientPortalPasswordAsync(int patientId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
