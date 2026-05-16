using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client.Services;

public interface IHospitalApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> LoginDoctorPortalAsync(DoctorPortalLoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> LoginPatientPortalAsync(PatientPortalLoginRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<DoctorDto?> GetDoctorPortalMeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDto>> GetDoctorPortalAppointmentsAsync(CancellationToken cancellationToken = default);

    Task<PatientDto?> GetPatientPortalMeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDto>> GetPatientPortalAppointmentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicDto>> GetPatientPortalClinicsAsync(string? search, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorDto>> GetPatientPortalDoctorsAsync(int clinicId, CancellationToken cancellationToken = default);

    Task<AppointmentDto?> PatientPortalBookAppointmentAsync(PortalPatientBookRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimelineEntryDto>> GetPatientPortalTimelineAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LabReportDto>> GetPatientPortalLabReportsAsync(CancellationToken cancellationToken = default);

    Task<PatientDto?> DoctorPortalLookupPatientAsync(string nationalId, CancellationToken cancellationToken = default);

    Task<AppointmentDto?> DoctorPortalBookAppointmentAsync(PortalDoctorBookRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimelineEntryDto>> GetDoctorPortalTimelineAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LabReportDto>> GetDoctorPortalPatientLabsAsync(int patientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorDto>> GetClinicDoctorsAsync(int clinicId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LabReportDto>> GetLabReportsAsync(int? patientId, CancellationToken cancellationToken = default);

    Task<LabReportDto?> GetLabReportAsync(int id, CancellationToken cancellationToken = default);

    Task<LabReportDto?> CreateLabReportAsync(CreateLabReportRequest request, CancellationToken cancellationToken = default);

    Task UpdateLabReportAsync(int id, UpdateLabReportRequest request, CancellationToken cancellationToken = default);

    Task DeleteLabReportAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorDutyDto>> GetDoctorDutiesAsync(int? doctorId, int? clinicId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    Task<DoctorDutyDto?> GetDoctorDutyAsync(int id, CancellationToken cancellationToken = default);

    Task<DoctorDutyDto?> CreateDoctorDutyAsync(CreateDoctorDutyRequest request, CancellationToken cancellationToken = default);

    Task UpdateDoctorDutyAsync(int id, UpdateDoctorDutyRequest request, CancellationToken cancellationToken = default);

    Task DeleteDoctorDutyAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PatientDto>> GetPatientsAsync(string? search, string? nationalId, CancellationToken cancellationToken = default);
    Task<PatientDto?> GetPatientAsync(int id, CancellationToken cancellationToken = default);
    Task<PatientDto?> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    Task UpdatePatientAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default);
    Task DeletePatientAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicDto>> GetClinicsAsync(string? search, CancellationToken cancellationToken = default);
    Task<ClinicDto?> CreateClinicAsync(CreateClinicRequest request, CancellationToken cancellationToken = default);
    Task UpdateClinicAsync(int id, UpdateClinicRequest request, CancellationToken cancellationToken = default);
    Task DeleteClinicAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorDto>> GetDoctorsAsync(string? search, int? clinicId, CancellationToken cancellationToken = default);
    Task<DoctorDto?> CreateDoctorAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default);
    Task UpdateDoctorAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default);
    Task DeleteDoctorAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(
        int? patientId,
        int? doctorId,
        int? clinicId,
        DateTime? from,
        DateTime? to,
        AppointmentStatus? status,
        CancellationToken cancellationToken = default);

    Task<AppointmentDto?> GetAppointmentAsync(int id, CancellationToken cancellationToken = default);

    Task<AppointmentDto?> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task UpdateAppointmentAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAppointmentAsync(int id, CancellationToken cancellationToken = default);
}
