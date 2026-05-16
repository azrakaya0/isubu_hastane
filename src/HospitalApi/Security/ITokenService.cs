using HospitalApi.Entities;

namespace HospitalApi.Security;

public interface ITokenService
{
    string CreateStaffToken(AppUser user);

    string CreateDoctorToken(Doctor doctor);

    string CreatePatientToken(Patient patient);
}
