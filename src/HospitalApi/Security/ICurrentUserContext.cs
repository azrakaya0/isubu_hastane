namespace HospitalApi.Security;

public interface ICurrentUserContext
{
    int? UserId { get; }
}
