using System.Security.Claims;
using Hospital.Shared.Dtos;
using HospitalApi.Services;
using HospitalApi.Validation;

namespace HospitalApi.Endpoints;

public static class HospitalEndpoints
{
    public static void MapHospitalEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var result = await authService.LoginAsync(request);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous();

        app.MapPost("/api/auth/login/doctor", async (DoctorPortalLoginRequest request, IAuthService authService) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var result = await authService.LoginDoctorPortalAsync(request);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous();

        app.MapPost("/api/auth/login/patient", async (PatientPortalLoginRequest request, IAuthService authService) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var result = await authService.LoginPatientPortalAsync(request);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous();

        app.MapPut("/api/auth/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, IAuthService authService) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var id = GetUserId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var (success, error) = await authService.ChangePasswordAsync(id.Value, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        }).RequireAuthorization("Admin");

        var api = app.MapGroup("/api").RequireAuthorization("Admin");

        api.MapGet("/patients", async (string? search, string? nationalId, IPatientService patients) =>
        {
            var list = await patients.GetAsync(new PatientListQuery { Search = search, NationalId = nationalId });
            return Results.Ok(list);
        });

        api.MapGet("/patients/{id:int}", async (int id, IPatientService patients) =>
        {
            var item = await patients.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/patients", async (CreatePatientRequest request, IPatientService patients) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error, patient) = await patients.CreateAsync(request);
            return success ? Results.Created($"/api/patients/{patient!.Id}", patient) : Results.BadRequest(new { error });
        });

        api.MapPut("/patients/{id:int}", async (int id, UpdatePatientRequest request, IPatientService patients) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error) = await patients.UpdateAsync(id, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapDelete("/patients/{id:int}", async (int id, IPatientService patients) =>
        {
            var (success, error) = await patients.DeleteAsync(id);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapGet("/clinics", async (string? search, IClinicService clinics) =>
        {
            var list = await clinics.GetAsync(new ClinicListQuery { Search = search });
            return Results.Ok(list);
        });

        api.MapGet("/clinics/{id:int}", async (int id, IClinicService clinics) =>
        {
            var item = await clinics.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapGet("/clinics/{id:int}/doctors", async (int id, IDoctorService doctors) =>
        {
            var list = await doctors.GetAsync(new DoctorListQuery { ClinicId = id });
            return Results.Ok(list);
        });

        api.MapPost("/clinics", async (CreateClinicRequest request, IClinicService clinics) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            try
            {
                var created = await clinics.CreateAsync(request);
                return Results.Created($"/api/clinics/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        api.MapPut("/clinics/{id:int}", async (int id, UpdateClinicRequest request, IClinicService clinics) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error) = await clinics.UpdateAsync(id, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapDelete("/clinics/{id:int}", async (int id, IClinicService clinics) =>
        {
            var (success, error) = await clinics.DeleteAsync(id);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapGet("/doctors", async (string? search, int? clinicId, IDoctorService doctors) =>
        {
            var list = await doctors.GetAsync(new DoctorListQuery { Search = search, ClinicId = clinicId });
            return Results.Ok(list);
        });

        api.MapGet("/doctors/{id:int}", async (int id, IDoctorService doctors) =>
        {
            var item = await doctors.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/doctors", async (CreateDoctorRequest request, IDoctorService doctors) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error, doctor) = await doctors.CreateAsync(request);
            return success ? Results.Created($"/api/doctors/{doctor!.Id}", doctor) : Results.BadRequest(new { error });
        });

        api.MapPut("/doctors/{id:int}", async (int id, UpdateDoctorRequest request, IDoctorService doctors) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error) = await doctors.UpdateAsync(id, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapDelete("/doctors/{id:int}", async (int id, IDoctorService doctors) =>
        {
            var (success, error) = await doctors.DeleteAsync(id);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapGet("/appointments", async (
            int? patientId,
            int? doctorId,
            int? clinicId,
            DateTime? from,
            DateTime? to,
            Hospital.Shared.Enums.AppointmentStatus? status,
            IAppointmentService appointments) =>
        {
            var list = await appointments.GetAsync(new AppointmentListQuery
            {
                PatientId = patientId,
                DoctorId = doctorId,
                ClinicId = clinicId,
                From = from,
                To = to,
                Status = status
            });
            return Results.Ok(list);
        });

        api.MapGet("/appointments/{id:int}", async (int id, IAppointmentService appointments) =>
        {
            var item = await appointments.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapGet("/appointments/slots", async (
            int doctorId,
            DateTime date,
            int? excludeAppointmentId,
            IAppointmentService appointments) =>
        {
            if (doctorId <= 0)
            {
                return Results.BadRequest(new { error = "Geçerli doktor seçiniz." });
            }

            var slots = await appointments.GetAvailableSlotsAsync(doctorId, date, excludeAppointmentId);
            return Results.Ok(slots);
        });

        api.MapPost("/appointments", async (CreateAppointmentRequest request, IAppointmentService appointments) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error, appointment) = await appointments.CreateAsync(request);
            return success
                ? Results.Created($"/api/appointments/{appointment!.Id}", appointment)
                : Results.BadRequest(new { error });
        });

        api.MapPut("/appointments/{id:int}", async (int id, UpdateAppointmentRequest request, IAppointmentService appointments) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error) = await appointments.UpdateAsync(id, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapDelete("/appointments/{id:int}", async (int id, IAppointmentService appointments) =>
        {
            var (success, error) = await appointments.DeleteAsync(id);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapGet("/lab-reports", async (int? patientId, ILabReportService labReports) =>
        {
            var list = await labReports.GetAllAsync(patientId);
            return Results.Ok(list);
        });

        api.MapGet("/lab-reports/{id:int}", async (int id, ILabReportService labReports) =>
        {
            var item = await labReports.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapGet("/lab-reports/{id:int}/pdf", async (int id, ILabReportService labReports) =>
        {
            var (content, fileName) = await labReports.GetPdfAsync(id);
            return content is null
                ? Results.NotFound()
                : Results.File(content, "application/pdf", fileName ?? "rapor.pdf");
        });

        api.MapPost("/lab-reports", async (CreateLabReportRequest request, ILabReportService labReports) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error, report) = await labReports.CreateAsync(request);
            return success ? Results.Created($"/api/lab-reports/{report!.Id}", report) : Results.BadRequest(new { error });
        });

        api.MapPut("/lab-reports/{id:int}", async (int id, UpdateLabReportRequest request, ILabReportService labReports) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error) = await labReports.UpdateAsync(id, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapDelete("/lab-reports/{id:int}", async (int id, ILabReportService labReports) =>
        {
            var (success, error) = await labReports.DeleteAsync(id);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapGet("/doctor-duties", async (int? doctorId, int? clinicId, DateTime? from, DateTime? to, IDoctorDutyService duties) =>
        {
            var list = await duties.GetAsync(new DoctorDutyListQuery
            {
                DoctorId = doctorId,
                ClinicId = clinicId,
                From = from,
                To = to
            });
            return Results.Ok(list);
        });

        api.MapGet("/doctor-duties/{id:int}", async (int id, IDoctorDutyService duties) =>
        {
            var item = await duties.GetByIdAsync(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        api.MapPost("/doctor-duties", async (CreateDoctorDutyRequest request, IDoctorDutyService duties) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error, duty) = await duties.CreateAsync(request);
            return success ? Results.Created($"/api/doctor-duties/{duty!.Id}", duty) : Results.BadRequest(new { error });
        });

        api.MapPut("/doctor-duties/{id:int}", async (int id, UpdateDoctorDutyRequest request, IDoctorDutyService duties) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var (success, error) = await duties.UpdateAsync(id, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        api.MapDelete("/doctor-duties/{id:int}", async (int id, IDoctorDutyService duties) =>
        {
            var (success, error) = await duties.DeleteAsync(id);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });
    }

    private static int? GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var value) ? value : null;
    }
}
