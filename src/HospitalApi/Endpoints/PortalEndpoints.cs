using System.Security.Claims;
using Hospital.Shared.Dtos;
using HospitalApi.Services;
using HospitalApi.Validation;

namespace HospitalApi.Endpoints;

public static class PortalEndpoints
{
    public static void MapPortalEndpoints(this WebApplication app)
    {
        var doctor = app.MapGroup("/api/portal/doctor").RequireAuthorization("Doctor");
        doctor.MapGet("/me", async (ClaimsPrincipal user, IDoctorService doctors) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var dto = await doctors.GetByIdAsync(id.Value);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        doctor.MapGet("/appointments", async (ClaimsPrincipal user, IAppointmentService appointments) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var list = await appointments.GetAsync(new AppointmentListQuery { DoctorId = id });
            return Results.Ok(list);
        });

        doctor.MapPost("/appointments", async (PortalDoctorBookRequest request, ClaimsPrincipal user, IPortalAppointmentService portal) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var (success, error, appt) = await portal.DoctorBookAsync(id.Value, request);
            return success
                ? Results.Created($"/api/portal/doctor/appointments/{appt!.Id}", appt)
                : Results.BadRequest(new { error });
        });

        doctor.MapGet("/timeline", async (ClaimsPrincipal user, IPortalTimelineService timeline) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var list = await timeline.GetDoctorTimelineAsync(id.Value);
            return Results.Ok(list);
        });

        doctor.MapGet("/patients/lookup", async (string? nationalId, IPatientService patients) =>
        {
            if (string.IsNullOrWhiteSpace(nationalId))
            {
                return Results.BadRequest(new { error = "T.C. kimlik numarası gerekli." });
            }

            var nid = nationalId.Trim();
            if (nid.Length != 11)
            {
                return Results.BadRequest(new { error = "T.C. kimlik numarası 11 haneli olmalıdır." });
            }

            var list = await patients.GetAsync(new PatientListQuery { NationalId = nid });
            var p = list.FirstOrDefault();
            return p is null ? Results.NotFound() : Results.Ok(p);
        });

        doctor.MapGet("/patient/{patientId:int}/lab-reports", async (int patientId, ClaimsPrincipal user, ILabReportService labs) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var list = await labs.GetByPatientForDoctorAsync(patientId, id.Value);
            return Results.Ok(list);
        });

        doctor.MapPut("/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, IAuthService auth) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var (success, error) = await auth.ChangeDoctorPortalPasswordAsync(id.Value, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });

        var patient = app.MapGroup("/api/portal/patient").RequireAuthorization("Patient");
        patient.MapGet("/me", async (ClaimsPrincipal user, IPatientService patients) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var dto = await patients.GetByIdAsync(id.Value);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        });

        patient.MapGet("/clinics", async (string? search, IClinicService clinics) =>
        {
            var list = await clinics.GetAsync(new ClinicListQuery { Search = search });
            return Results.Ok(list);
        });

        patient.MapGet("/doctors", async (int clinicId, IDoctorService doctors) =>
        {
            if (clinicId <= 0)
            {
                return Results.BadRequest(new { error = "Geçerli poliklinik seçiniz." });
            }

            var list = await doctors.GetAsync(new DoctorListQuery { ClinicId = clinicId });
            return Results.Ok(list);
        });

        patient.MapGet("/appointments", async (ClaimsPrincipal user, IAppointmentService appointments) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var list = await appointments.GetAsync(new AppointmentListQuery { PatientId = id });
            return Results.Ok(list);
        });

        patient.MapPost("/appointments", async (PortalPatientBookRequest request, ClaimsPrincipal user, IPortalAppointmentService portal) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var (success, error, appt) = await portal.PatientBookAsync(id.Value, request);
            return success
                ? Results.Created($"/api/portal/patient/appointments/{appt!.Id}", appt)
                : Results.BadRequest(new { error });
        });

        patient.MapGet("/timeline", async (ClaimsPrincipal user, IPortalTimelineService timeline) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var list = await timeline.GetPatientTimelineAsync(id.Value);
            return Results.Ok(list);
        });

        patient.MapGet("/lab-reports", async (ClaimsPrincipal user, ILabReportService labs) =>
        {
            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var list = await labs.GetByPatientAsync(id.Value);
            return Results.Ok(list);
        });

        patient.MapPut("/change-password", async (ChangePasswordRequest request, ClaimsPrincipal user, IAuthService auth) =>
        {
            var validation = RequestValidator.Validate(request);
            if (validation is not null)
            {
                return validation;
            }

            var id = GetSubjectId(user);
            if (id is null)
            {
                return Results.Unauthorized();
            }

            var (success, error) = await auth.ChangePatientPortalPasswordAsync(id.Value, request);
            return success ? Results.NoContent() : Results.BadRequest(new { error });
        });
    }

    private static int? GetSubjectId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var value) ? value : null;
    }
}
