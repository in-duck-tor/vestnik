namespace InDuckTor.Vestnik.Domain;

public class ClientAppRegistration
{
    public int Id { get; init; }
    public required string ApplicationId { get; init; }
    public required string RegistrationToken { get; init; }
    public string? DeviceId { get; set; }
    public int? UserId { get; set; }
    public required DateTime RegisteredAt { get; init; }
    public DateTime? ExpiresAt { get; set; }
}