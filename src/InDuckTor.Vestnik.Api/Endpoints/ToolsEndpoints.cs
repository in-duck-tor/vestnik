using InDuckTor.Vestnik.Domain.Messaging;
using InDuckTor.Vestnik.Infrastructure.Database;
using InDuckTor.Vestnik.Infrastructure.Firebase;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Api.Endpoints;

public static class ToolsEndpoints
{
    public static void AddToolsEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/vestnik/tool")
            .WithTags("Tool\\Debug")
            .RequireAuthorization("SystemAccess")
            .WithOpenApi(operation =>
            {
                operation.Deprecated = true;
                return operation;
            });

        groupBuilder.MapPost("/send-test-message/user/{userId}", SendTestMessageToUser)
            .WithName(nameof(SendTestMessageToUser));
    }

    internal static async Task<JsonHttpResult<object>> SendTestMessageToUser(int userId, VestnikDbContext dbContext, IInDuckTorBankMessageSender messageSender, CancellationToken ct)
    {
        var usersRegistrations = await dbContext.ClientAppRegistrations.Where(x => x.UserId == userId)
            .AsNoTracking()
            .ToListAsync(ct);

        var results = await messageSender.SendSimpleNotification(
            new NotificationDataBase("Тестовое", $"Timestamp : {DateTime.UtcNow}", "https://cdnito.tomsk.ru/wp-content/uploads/2017/10/4204_1_13_1s.jpg"),
            usersRegistrations.Select(x => x.RegistrationToken), ct);
        
        return TypedResults.Json((object) new { results.successMessageIds, results.failedMessageIds, results.unprocessableMessageIds });
    }
}