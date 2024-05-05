using FluentResults;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using Microsoft.AspNetCore.SignalR;

namespace InDuckTor.Vestnik.Features.Account;

public class AccountEventsHandler :
    IMulticastCommandHandler<AccountCreatedEvent>,
    IMulticastCommandHandler<AccountUpdatedEvent>,
    IMulticastCommandHandler<TransactionCreatedEvent>,
    IMulticastCommandHandler<TransactionUpdatedEvent>
{
    private readonly IHubContext<AccountEventsHub, IAccountEventsHub> _hubContext;

    public AccountEventsHandler(IHubContext<AccountEventsHub, IAccountEventsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<Result> Execute(AccountCreatedEvent @event, CancellationToken ct)
    {
        var userIds = @event.GrantedUsers
            .Select(user => user.id)
            .Append(@event.CreatedById)
            .Append(@event.OwnerId)
            .Select(userId => userId.ToString());
        var clientProxy = _hubContext.Clients.Users(userIds);

        await clientProxy.AccountCreated(@event, ct);

        return Result.Ok();
    }

    public async Task<Result> Execute(AccountUpdatedEvent @event, CancellationToken ct)
    {
        var clientProxy = _hubContext.Clients.Group(AccountGroup.GetAccountGroupName(@event.AccountNumber));
        await clientProxy.AccountUpdatedEvent(@event, ct);
        return Result.Ok();
    }

    public async Task<Result> Execute(TransactionCreatedEvent @event, CancellationToken ct)
    {
        var clientProxy1 = @event.DepositOn == null
            ? null
            : _hubContext.Clients.Group(AccountGroup.GetAccountGroupName(@event.DepositOn.AccountNumber));
        var clientProxy2 = @event.WithdrawFrom == null
            ? null
            : _hubContext.Clients.Group(AccountGroup.GetAccountGroupName(@event.WithdrawFrom.AccountNumber));

        var task1 = clientProxy1?.TransactionCreatedEvent(@event, ct);
        var task2 = clientProxy2?.TransactionCreatedEvent(@event, ct);

        if (task1 != null) await task1;
        if (task2 != null) await task2;

        return Result.Ok();
    }

    public async Task<Result> Execute(TransactionUpdatedEvent @event, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}