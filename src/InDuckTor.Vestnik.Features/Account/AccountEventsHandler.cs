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
        var clientProxy = _hubContext.Clients.Users(
            @event.GrantedUsers
                .Select(user => user.id)
                .Append(@event.CreatedById)
                .Append(@event.OwnerId)
                .Select(userId => userId.ToString()));

        await clientProxy.AccountCreated(@event, ct);

        return Result.Ok();
    }

    public async Task<Result> Execute(AccountUpdatedEvent @event, CancellationToken ct)
    {
        _hubContext.Clients.Clients(default!);
        return Result.Ok();
    }

    public async Task<Result> Execute(TransactionCreatedEvent @event, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> Execute(TransactionUpdatedEvent @event, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}