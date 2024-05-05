using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;
using InDuckTor.Vestnik.Infrastructure.Database;
using InDuckTor.Vestnik.Infrastructure.Firebase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Features.Messaging.Services;

public interface IMessageSenderService
{
    Task SendNotificationTo(ApplicationVariant application, IEnumerable<ClientAppRegistration> registrations, NotificationDataBase notification, CancellationToken ct);
    Task SendManyNotificationsTo(ApplicationVariant application, IEnumerable<(ClientAppRegistration registration, NotificationDataBase data)> notifications, CancellationToken ct);
}

public class MessageSenderService : IMessageSenderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageSenderService> _logger;

    public MessageSenderService(IServiceProvider serviceProvider, ILogger<MessageSenderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendNotificationTo(ApplicationVariant application, IEnumerable<ClientAppRegistration> registrations, NotificationDataBase notification, CancellationToken ct)
    {
        var sender = _serviceProvider.GetRequiredKeyedService<IMessageSender>(application);
        var result = await sender.SendSimpleNotification(notification, registrations, ct);

        if (result.UnprocessableRegistrations.Count > 0)
        {
            await DropRegistrations(result.UnprocessableRegistrations, ct);
        }

        if (result.FailedDispatchRegistrations.Count > 0)
        {
            // todo retry или что-то ? 
        }
    }

    public async Task SendManyNotificationsTo(ApplicationVariant application, IEnumerable<(ClientAppRegistration registration, NotificationDataBase data)> notifications, CancellationToken ct)
    {
        var sender = _serviceProvider.GetRequiredKeyedService<IMessageSender>(application);
        var result = await sender.SendManySimpleNotifications(notifications, ct);

        if (result.UnprocessableRegistrations.Count > 0)
        {
            await DropRegistrations(result.UnprocessableRegistrations, ct);
        }

        if (result.FailedDispatchRegistrations.Count > 0)
        {
            // todo retry или что-то ? 
        }
    }

    private async Task DropRegistrations(IReadOnlyCollection<ClientAppRegistration> registrations, CancellationToken ct)
    {
        var dbContext = _serviceProvider.GetRequiredService<VestnikDbContext>();
        var registrationIds = registrations.Select(x => x.Id);

        await dbContext.ClientAppRegistrations
            .Where(x => registrationIds.Contains(x.Id))
            .ExecuteDeleteAsync(ct);

        _logger.LogInformation("Удалено {UnprocessableRegistrationsCount} невалидных токенов регистрации", registrations.Count);
    }
}