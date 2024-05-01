using InDuckTor.Shared.Security.Context;
using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Infrastructure.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Api.Endpoints;

public static class ClientAppRegistrationEndpoints
{
    public static IEndpointRouteBuilder AddClientAppRegistrationEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1/ws/vestnik/app-registration")
            .WithTags("ClientAppRegistration")
            .WithOpenApi();

        groupBuilder.MapPut("/", SetClientAppRegistration)
            .WithName(nameof(SetClientAppRegistration))
            .WithDescription("Регистрирует кдиентское прилложение в системе. Дедуплицирует существующие регистрации");

        return builder;
    }

    public record ClientAppRegistrationRequest(string RegistrationToken, DateTime? RegisteredAt, string ApplicationId, string? DeviceId);

    internal static async Task<NoContent> SetClientAppRegistration(ClientAppRegistrationRequest request, ISecurityContext securityContext, VestnikDbContext dbContext, CancellationToken ct)
    {
        var registration = new ClientAppRegistration
        {
            ApplicationId = request.ApplicationId,
            RegistrationToken = request.RegistrationToken,
            RegisteredAt = request.RegisteredAt ?? DateTime.UtcNow,
            DeviceId = request.DeviceId,
            UserId = securityContext.Currant.Id,
        };

        registration.ExpiresAt = registration.RegisteredAt.AddDays(30);

        await using (var tx = await dbContext.Database.BeginTransactionAsync(ct))
        {
            await dbContext.ClientAppRegistrations
                .Where(x => x.ApplicationId == registration.ApplicationId
                            && x.UserId == registration.UserId
                            && x.DeviceId == registration.DeviceId)
                .ExecuteDeleteAsync(ct);

            dbContext.ClientAppRegistrations.Add(registration);
            await dbContext.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
        }

        return TypedResults.NoContent();
    }
}