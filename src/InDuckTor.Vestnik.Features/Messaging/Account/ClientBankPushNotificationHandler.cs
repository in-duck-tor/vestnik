using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;
using InDuckTor.Vestnik.Features.Messaging.Services;
using InDuckTor.Vestnik.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Features.Messaging.Account;

public class ClientBankPushNotificationHandler :
    IMulticastCommandHandler<AccountCreatedEvent>,
    IMulticastCommandHandler<AccountUpdatedEvent>
{
    private readonly IMessageSenderService _messageSenderService;
    private readonly VestnikDbContext _dbContext;

    public ClientBankPushNotificationHandler(IMessageSenderService messageSenderService, VestnikDbContext dbContext)
    {
        _messageSenderService = messageSenderService;
        _dbContext = dbContext;
    }

    public async Task<Result> Execute(AccountCreatedEvent @event, CancellationToken ct)
    {
        if (@event.AccountType != AccountType.Payment) return Result.Ok();
        
        // пока шлём только владельцу
        // var relatedUsers = @event.GrantedUsers.Select(x => x.id).Append(@event.OwnerId).Append(@event.CreatedById).Distinct();

        var registrations = await _dbContext.ClientAppRegistrations
            .Where(x => x.ApplicationId == ApplicationVariant.ClientBank && x.UserId == @event.OwnerId)
            .AsNoTracking()
            .ToListAsync(ct);

        await _messageSenderService.SendNotificationTo(ApplicationVariant.ClientBank,
            registrations,
            AccountClientMessageFactory.NotificationFromCreated(@event), ct);

        return Result.Ok();
    }

    public async Task<Result> Execute(AccountUpdatedEvent @event, CancellationToken ct)
    {
        if (@event.AccountType != AccountType.Payment) return Result.Ok();
        
        // пока шлём только тому кто изменил

        var registrations = await _dbContext.ClientAppRegistrations
            .Where(x => x.ApplicationId == ApplicationVariant.ClientBank
                        && x.UserId == @event.ChangedById
                // todo : здесь нужно ещё родить как-то владельца 
            )
            .AsNoTracking()
            .ToListAsync(ct);

        await _messageSenderService.SendNotificationTo(ApplicationVariant.ClientBank,
            registrations,
            AccountClientMessageFactory.NotificationFromUpdated(@event), ct);

        return Result.Ok();
        ;
    }
}