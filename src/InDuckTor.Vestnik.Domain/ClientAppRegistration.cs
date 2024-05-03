using System.Runtime.Serialization;

namespace InDuckTor.Vestnik.Domain;

public class ClientAppRegistration
{
    public int Id { get; init; }
    public required ApplicationVariant ApplicationId { get; init; }
    public required string RegistrationToken { get; init; }
    public string? DeviceId { get; set; }
    public int? UserId { get; set; }
    public required DateTime RegisteredAt { get; init; }
    public DateTime? ExpiresAt { get; set; }
}

public enum ApplicationVariant
{
    [EnumMember(Value = "inductorbank")]
    ClientBank = 1,
    [EnumMember(Value = "employee_inductorbank")]
    EmployeeWebInterface = 2
}