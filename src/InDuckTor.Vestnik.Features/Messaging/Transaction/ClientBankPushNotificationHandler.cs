using FluentResults;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;
using InDuckTor.Vestnik.Features.Messaging.Services;
using InDuckTor.Vestnik.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Features.Messaging.Transaction;

public class ClientBankPushNotificationHandler : IMulticastCommandHandler<TransactionCreatedEvent>
{
    private readonly IMessageSenderService _messageSenderService;
    private readonly VestnikDbContext _dbContext;

    public ClientBankPushNotificationHandler(IMessageSenderService messageSenderService, VestnikDbContext dbContext)
    {
        _messageSenderService = messageSenderService;
        _dbContext = dbContext;
    }

    public async Task<Result> Execute(TransactionCreatedEvent @event, CancellationToken ct)
    {
        var notificationsForUser = TransactionClientMessageFactory.NotificationsFromStarted(@event).ToList();

        var users = notificationsForUser.Select(x => x.userId);
        var usersRegistrations = await _dbContext.ClientAppRegistrations
            .Where(x => x.ApplicationId == ApplicationVariant.ClientBank
                        && x.UserId != null
                        && users.Contains(x.UserId.Value))
            .AsNoTracking()
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(g => g.Key!.Value, g => g.ToArray(), ct);


        var joinedNotificationsWithRegistrations = notificationsForUser
            .Where(notificationForUser => usersRegistrations.ContainsKey(notificationForUser.userId))
            .SelectMany(NotificationForEachUser);

        await _messageSenderService.SendManyNotificationsTo(ApplicationVariant.ClientBank, joinedNotificationsWithRegistrations, ct);

        return Result.Ok();

        IEnumerable<(ClientAppRegistration registration, NotificationDataBase notification)> NotificationForEachUser((int userId, NotificationDataBase notification) notificationForUser)
            => usersRegistrations[notificationForUser.userId]
                .Select(registration => (registration, notificationForUser.notification));
    }
}