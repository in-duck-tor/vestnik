using FluentResults;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;
using InDuckTor.Vestnik.Features.Messaging.Services;
using InDuckTor.Vestnik.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Features.Messaging.Account;

/// <summary>
/// отправляем вообще все события вообще всем сотрудникам  -  таковы требования
/// </summary>
public class EmployeeWebInterfacePushNotificationHandler :
    IMulticastCommandHandler<AccountCreatedEvent>,
    IMulticastCommandHandler<AccountUpdatedEvent>
{
    private readonly IMessageSenderService _messageSenderService;
    private readonly VestnikDbContext _dbContext;

    public EmployeeWebInterfacePushNotificationHandler(IMessageSenderService messageSenderService, VestnikDbContext dbContext)
    {
        _messageSenderService = messageSenderService;
        _dbContext = dbContext;
    }

    public async Task<Result> Execute(AccountCreatedEvent @event, CancellationToken ct)
    {
        var registrations = await _dbContext.ClientAppRegistrations
            .Where(x => x.ApplicationId == ApplicationVariant.EmployeeWebInterface)
            .AsNoTracking()
            .ToListAsync(ct);

        await _messageSenderService.SendNotificationTo(ApplicationVariant.ClientBank,
            registrations,
            AccountEmployeeMessageFactory.NotificationFromCreated(@event), ct);

        return Result.Ok();
    }

    public async Task<Result> Execute(AccountUpdatedEvent @event, CancellationToken ct)
    {
        var registrations = await _dbContext.ClientAppRegistrations
            .Where(x => x.ApplicationId == ApplicationVariant.EmployeeWebInterface)
            .AsNoTracking()
            .ToListAsync(ct);

        await _messageSenderService.SendNotificationTo(ApplicationVariant.ClientBank,
            registrations,
            AccountEmployeeMessageFactory.NotificationFromUpdated(@event), ct);

        return Result.Ok();
        ;
    }
}