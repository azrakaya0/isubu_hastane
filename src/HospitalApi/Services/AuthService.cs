using Hospital.Shared.Dtos;
using HospitalApi.Data;
using HospitalApi.Entities;
using HospitalApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HospitalApi.Services;

public sealed class AuthService(
    HospitalDbContext db,
    IPasswordHasher<AppUser> passwordHasher,
    IPasswordHasher<Doctor> doctorPasswordHasher,
    IPasswordHasher<Patient> patientPasswordHasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = tokenService.CreateStaffToken(user);
        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Role = "Admin"
        };
    }

    public async Task<LoginResponse?> LoginDoctorPortalAsync(DoctorPortalLoginRequest request, CancellationToken cancellationToken = default)
    {
        var doctor = await db.Doctors.AsNoTracking()
            .FirstOrDefaultAsync(d => d.PortalUserName == request.UserName, cancellationToken);

        if (doctor is null || string.IsNullOrEmpty(doctor.PortalPasswordHash))
        {
            return null;
        }

        var verification = doctorPasswordHasher.VerifyHashedPassword(doctor, doctor.PortalPasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = tokenService.CreateDoctorToken(doctor);
        return new LoginResponse
        {
            Token = token,
            UserId = doctor.Id,
            UserName = doctor.PortalUserName!,
            FullName = $"{doctor.FirstName} {doctor.LastName}",
            Role = "Doctor"
        };
    }

    public async Task<LoginResponse?> LoginPatientPortalAsync(PatientPortalLoginRequest request, CancellationToken cancellationToken = default)
    {
        var nationalId = request.NationalId.Trim();
        var patient = await db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.NationalId == nationalId, cancellationToken);

        if (patient is null || string.IsNullOrEmpty(patient.PortalPasswordHash))
        {
            return null;
        }

        var verification = patientPasswordHasher.VerifyHashedPassword(patient, patient.PortalPasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = tokenService.CreatePatientToken(patient);
        return new LoginResponse
        {
            Token = token,
            UserId = patient.Id,
            UserName = patient.NationalId,
            FullName = $"{patient.FirstName} {patient.LastName}",
            Role = "Patient"
        };
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return (false, "Kullanıcı bulunamadı.");
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return (false, "Mevcut şifre hatalı.");
        }

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeDoctorPortalPasswordAsync(int doctorId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var doctor = await db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);
        if (doctor is null || string.IsNullOrEmpty(doctor.PortalPasswordHash))
        {
            return (false, "Doktor portal hesabı bulunamadı.");
        }

        var verification = doctorPasswordHasher.VerifyHashedPassword(doctor, doctor.PortalPasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return (false, "Mevcut şifre hatalı.");
        }

        doctor.PortalPasswordHash = doctorPasswordHasher.HashPassword(doctor, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePatientPortalPasswordAsync(int patientId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (patient is null || string.IsNullOrEmpty(patient.PortalPasswordHash))
        {
            return (false, "Hasta portal hesabı bulunamadı.");
        }

        var verification = patientPasswordHasher.VerifyHashedPassword(patient, patient.PortalPasswordHash, request.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return (false, "Mevcut şifre hatalı.");
        }

        patient.PortalPasswordHash = patientPasswordHasher.HashPassword(patient, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
