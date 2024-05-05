using FluentResults;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using InDuckTor.Vestnik.Domain.Messaging;
using InDuckTor.Vestnik.Features.Messaging.Services;
using InDuckTor.Vestnik.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Features.Messaging.Transaction;

/// <summary>
/// отправляем вообще все события вообще всем сотрудникам  -  таковы требования
/// </summary>
public class EmployeeWebInterfacePushNotificationHandler : IMulticastCommandHandler<TransactionCreatedEvent>
{
    private readonly IMessageSenderService _messageSenderService;
    private readonly VestnikDbContext _dbContext;

    public EmployeeWebInterfacePushNotificationHandler(IMessageSenderService messageSenderService, VestnikDbContext dbContext)
    {
        _messageSenderService = messageSenderService;
        _dbContext = dbContext;
    }

    public async Task<Result> Execute(TransactionCreatedEvent @event, CancellationToken ct)
    {
        var notification = TransactionEmployeeMessageFactory.CreateNotificationFromStarted(@event);

        var registrations = await _dbContext.ClientAppRegistrations
            .Where(x => x.ApplicationId == ApplicationVariant.EmployeeWebInterface)
            .AsNoTracking()
            .ToListAsync(ct);

        await _messageSenderService.SendNotificationTo(ApplicationVariant.ClientBank, registrations, notification, ct);

        return Result.Ok();
    }
}