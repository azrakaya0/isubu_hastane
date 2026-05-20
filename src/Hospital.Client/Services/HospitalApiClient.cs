using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hospital.Shared.Dtos;
using Hospital.Shared.Enums;

namespace Hospital.Client.Services;

public sealed class HospitalApiClient(HttpClient http, IAuthTokenStore tokenStore) : IHospitalApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        tokenStore.Clear();
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        message.Headers.Authorization = null;

        using var response = await http.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        if (result is not null)
        {
            tokenStore.SetSession(result.Token, string.IsNullOrWhiteSpace(result.Role) ? "Admin" : result.Role, result.FullName);
        }

        return result;
    }

    public async Task<LoginResponse?> LoginDoctorPortalAsync(DoctorPortalLoginRequest request, CancellationToken cancellationToken = default)
    {
        tokenStore.Clear();
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/doctor")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        message.Headers.Authorization = null;

        using var response = await http.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        if (result is not null)
        {
            tokenStore.SetSession(result.Token, string.IsNullOrWhiteSpace(result.Role) ? "Doctor" : result.Role, result.FullName);
        }

        return result;
    }

    public async Task<LoginResponse?> LoginPatientPortalAsync(PatientPortalLoginRequest request, CancellationToken cancellationToken = default)
    {
        tokenStore.Clear();
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/login/patient")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        message.Headers.Authorization = null;

        using var response = await http.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        if (result is not null)
        {
            tokenStore.SetSession(result.Token, string.IsNullOrWhiteSpace(result.Role) ? "Patient" : result.Role, result.FullName);
        }

        return result;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var path = tokenStore.Role switch
        {
            "Doctor" => "api/portal/doctor/change-password",
            "Patient" => "api/portal/patient/change-password",
            _ => "api/auth/change-password"
        };

        using var response = await http.PutAsJsonAsync(path, request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task<DoctorDto?> GetDoctorPortalMeAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<DoctorDto>("api/portal/doctor/me", JsonOptions, cancellationToken);

    public async Task<IReadOnlyList<AppointmentDto>> GetDoctorPortalAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<AppointmentDto>>("api/portal/doctor/appointments", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<PatientDto?> GetPatientPortalMeAsync(CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<PatientDto>("api/portal/patient/me", JsonOptions, cancellationToken);

    public async Task<PatientDto?> UpdatePatientPortalProfileAsync(UpdatePatientProfileRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync("api/portal/patient/me/profile", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions, cancellationToken);
    }

    public async Task<Stream?> GetLabReportPdfStreamAsync(int reportId, bool patientPortal, CancellationToken cancellationToken = default)
    {
        var url = patientPortal
            ? $"api/portal/patient/lab-reports/{reportId}/pdf"
            : $"api/lab-reports/{reportId}/pdf";
        var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetPatientPortalAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<AppointmentDto>>("api/portal/patient/appointments", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<ClinicDto>> GetPatientPortalClinicsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/portal/patient/clinics", ("search", search));
        var list = await http.GetFromJsonAsync<List<ClinicDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<DoctorDto>> GetPatientPortalDoctorsAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/portal/patient/doctors", ("clinicId", clinicId.ToString()));
        var list = await http.GetFromJsonAsync<List<DoctorDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<AppointmentSlotDto>> GetPatientPortalAppointmentSlotsAsync(
        int doctorId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(
            "api/portal/patient/appointments/slots",
            ("doctorId", doctorId.ToString()),
            ("date", date.Date.ToString("yyyy-MM-dd")));
        var list = await http.GetFromJsonAsync<List<AppointmentSlotDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<AppointmentDto?> PatientPortalBookAppointmentAsync(PortalPatientBookRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/portal/patient/appointments", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<TimelineEntryDto>> GetPatientPortalTimelineAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<TimelineEntryDto>>("api/portal/patient/timeline", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<LabReportDto>> GetPatientPortalLabReportsAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<LabReportDto>>("api/portal/patient/lab-reports", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<PatientDto?> DoctorPortalLookupPatientAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/portal/doctor/patients/lookup", ("nationalId", nationalId.Trim()));
        using var response = await http.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentSlotDto>> GetDoctorPortalAppointmentSlotsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/portal/doctor/appointments/slots", ("date", date.Date.ToString("yyyy-MM-dd")));
        var list = await http.GetFromJsonAsync<List<AppointmentSlotDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<AppointmentDto?> DoctorPortalBookAppointmentAsync(PortalDoctorBookRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/portal/doctor/appointments", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<TimelineEntryDto>> GetDoctorPortalTimelineAsync(CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<TimelineEntryDto>>("api/portal/doctor/timeline", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<LabReportDto>> GetDoctorPortalPatientLabsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<LabReportDto>>($"api/portal/doctor/patient/{patientId}/lab-reports", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<DoctorDto>> GetClinicDoctorsAsync(int clinicId, CancellationToken cancellationToken = default)
    {
        var list = await http.GetFromJsonAsync<List<DoctorDto>>($"api/clinics/{clinicId}/doctors", JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<IReadOnlyList<LabReportDto>> GetLabReportsAsync(int? patientId, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/lab-reports", ("patientId", patientId?.ToString()));
        var list = await http.GetFromJsonAsync<List<LabReportDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<LabReportDto?> GetLabReportAsync(int id, CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<LabReportDto>($"api/lab-reports/{id}", JsonOptions, cancellationToken);

    public async Task<LabReportDto?> CreateLabReportAsync(CreateLabReportRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/lab-reports", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<LabReportDto>(JsonOptions, cancellationToken);
    }

    public async Task UpdateLabReportAsync(int id, UpdateLabReportRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/lab-reports/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task DeleteLabReportAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/lab-reports/{id}", cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DoctorDutyDto>> GetDoctorDutiesAsync(int? doctorId, int? clinicId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(
            "api/doctor-duties",
            ("doctorId", doctorId?.ToString()),
            ("clinicId", clinicId?.ToString()),
            ("from", from?.ToString("o")),
            ("to", to?.ToString("o")));
        var list = await http.GetFromJsonAsync<List<DoctorDutyDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<DoctorDutyDto?> GetDoctorDutyAsync(int id, CancellationToken cancellationToken = default) =>
        await http.GetFromJsonAsync<DoctorDutyDto>($"api/doctor-duties/{id}", JsonOptions, cancellationToken);

    public async Task<DoctorDutyDto?> CreateDoctorDutyAsync(CreateDoctorDutyRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/doctor-duties", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DoctorDutyDto>(JsonOptions, cancellationToken);
    }

    public async Task UpdateDoctorDutyAsync(int id, UpdateDoctorDutyRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/doctor-duties/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task DeleteDoctorDutyAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/doctor-duties/{id}", cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PatientDto>> GetPatientsAsync(string? search, string? nationalId, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/patients", ("search", search), ("nationalId", nationalId));
        var list = await http.GetFromJsonAsync<List<PatientDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<PatientDto?> GetPatientAsync(int id, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<PatientDto>($"api/patients/{id}", JsonOptions, cancellationToken);
    }

    public async Task<PatientDto?> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/patients", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions, cancellationToken);
    }

    public async Task UpdatePatientAsync(int id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/patients/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task DeletePatientAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/patients/{id}", cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ClinicDto>> GetClinicsAsync(string? search, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("api/clinics", ("search", search));
        var list = await http.GetFromJsonAsync<List<ClinicDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<ClinicDto?> CreateClinicAsync(CreateClinicRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/clinics", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ClinicDto>(JsonOptions, cancellationToken);
    }

    public async Task UpdateClinicAsync(int id, UpdateClinicRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/clinics/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task DeleteClinicAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/clinics/{id}", cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DoctorDto>> GetDoctorsAsync(string? search, int? clinicId, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(
            "api/doctors",
            ("search", search),
            ("clinicId", clinicId?.ToString()));
        var list = await http.GetFromJsonAsync<List<DoctorDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<DoctorDto?> CreateDoctorAsync(CreateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/doctors", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<DoctorDto>(JsonOptions, cancellationToken);
    }

    public async Task UpdateDoctorAsync(int id, UpdateDoctorRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/doctors/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task DeleteDoctorAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/doctors/{id}", cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(
        int? patientId,
        int? doctorId,
        int? clinicId,
        DateTime? from,
        DateTime? to,
        AppointmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(
            "api/appointments",
            ("patientId", patientId?.ToString()),
            ("doctorId", doctorId?.ToString()),
            ("clinicId", clinicId?.ToString()),
            ("from", from?.ToString("o")),
            ("to", to?.ToString("o")),
            ("status", status?.ToString()));
        var list = await http.GetFromJsonAsync<List<AppointmentDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<AppointmentDto?> GetAppointmentAsync(int id, CancellationToken cancellationToken = default)
    {
        return await http.GetFromJsonAsync<AppointmentDto>($"api/appointments/{id}", JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentSlotDto>> GetAppointmentSlotsAsync(
        int doctorId,
        DateTime date,
        int? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(
            "api/appointments/slots",
            ("doctorId", doctorId.ToString()),
            ("date", date.Date.ToString("yyyy-MM-dd")),
            ("excludeAppointmentId", excludeAppointmentId?.ToString()));
        var list = await http.GetFromJsonAsync<List<AppointmentSlotDto>>(url, JsonOptions, cancellationToken);
        return list ?? [];
    }

    public async Task<AppointmentDto?> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PostAsJsonAsync("api/appointments", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions, cancellationToken);
    }

    public async Task UpdateAppointmentAsync(int id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await http.PutAsJsonAsync($"api/appointments/{id}", request, JsonOptions, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task DeleteAppointmentAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await http.DeleteAsync($"api/appointments/{id}", cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    private static string BuildUrl(string path, params (string Name, string? Value)[] query)
    {
        var parts = query
            .Where(q => !string.IsNullOrWhiteSpace(q.Value))
            .Select(q => $"{q.Name}={Uri.EscapeDataString(q.Value!)}")
            .ToList();
        return parts.Count == 0 ? path : $"{path}?{string.Join("&", parts)}";
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
            ? $"İstek başarısız: {(int)response.StatusCode}"
            : body);
    }
}
