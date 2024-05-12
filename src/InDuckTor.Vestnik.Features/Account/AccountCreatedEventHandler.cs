using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Features.Account;

public class AccountCreatedEventHandler : IMulticastCommandHandler<AccountCreatedEvent>
{
    private readonly IHubContext<AccountEventsHub, IAccountEventsHub> _hubContext;

    public AccountCreatedEventHandler(IHubContext<AccountEventsHub, IAccountEventsHub> hubContext)
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
}

public class AccountUpdatedEventHandler : IMulticastCommandHandler<AccountUpdatedEvent>
{
    private readonly IHubContext<AccountEventsHub, IAccountEventsHub> _hubContext;

    public AccountUpdatedEventHandler(IHubContext<AccountEventsHub, IAccountEventsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<Result> Execute(AccountUpdatedEvent @event, CancellationToken ct)
    {
        var clientProxy = _hubContext.Clients.Group(AccountGroup.GetGroupName(@event.AccountNumber));
        await clientProxy.AccountUpdated(@event, ct);

        return Result.Ok();
    }
}

public class TransactionCreatedEventHandler : IMulticastCommandHandler<TransactionCreatedEvent>
{
    private readonly IHubContext<AccountEventsHub, IAccountEventsHub> _hubContext;
    private readonly ILogger<TransactionCreatedEventHandler> _logger;

    public TransactionCreatedEventHandler(
        IHubContext<AccountEventsHub, IAccountEventsHub> hubContext,
        ILogger<TransactionCreatedEventHandler> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<Result> Execute(TransactionCreatedEvent @event, CancellationToken ct)
    {
        _logger.Log(LogLevel.Debug, "Start processing TransactionCreatedEvent");

        List<string> accounts = [];

        if (@event.DepositOn != null)
        {
            accounts.Add(@event.DepositOn.AccountNumber);
            _logger.Log(LogLevel.Debug, "Added deposit account");
        }

        if (@event.WithdrawFrom != null)
        {
            accounts.Add(@event.WithdrawFrom.AccountNumber);
            _logger.Log(LogLevel.Debug, "Added withdraw account");
        }

        _logger.Log(LogLevel.Debug, "Invoking TransactionCreated client method");

        await _hubContext.Clients
            .Groups(accounts.Select(AccountGroup.GetGroupName))
            .TransactionCreated(@event, ct);

        _logger.Log(LogLevel.Debug, "TransactionCreated client method finished");

        if (!AccountEventsHub.TransactionToAccounts.TryAdd(@event.TransactionId, accounts))
        {
            _logger.Log(LogLevel.Error,
                "НЕ ПОЛУЧИЛОСЬ ДОБАВИТЬ АККАУНТЫ В ТРАНЗАКЦИЮ. TransactionId: {}", @event.TransactionId);
        }

        return Result.Ok();
    }
}

public class TransactionUpdatedEventHandler : IMulticastCommandHandler<TransactionUpdatedEvent>
{
    private readonly IHubContext<AccountEventsHub, IAccountEventsHub> _hubContext;

    public TransactionUpdatedEventHandler(IHubContext<AccountEventsHub, IAccountEventsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<Result> Execute(TransactionUpdatedEvent @event, CancellationToken ct)
    {
        AccountEventsHub.TransactionToAccounts.TryGetValue(@event.TransactionId, out var accounts);
        if (accounts == null) return Result.Fail($"Transaction with id '{@event.TransactionId}' not found");

        await Task.WhenAll(accounts.Select(account => _hubContext.Clients
            .Group(AccountGroup.GetGroupName(account))
            .TransactionUpdated(@event, ct)
        ));

        if (@event.Status is TransactionStatus.Canceled or TransactionStatus.Committed)
            AccountEventsHub.TransactionToAccounts.TryRemove(@event.TransactionId, out _);

        return Result.Ok();
    }
}

public interface IHueta : ICommand<Unit, Unit>;

public class Hueta : IHueta
{
    private readonly IHubContext<AccountEventsHub, IAccountEventsHub> _hubContext;

    public Hueta(IHubContext<AccountEventsHub, IAccountEventsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task<Unit> Execute(Unit input, CancellationToken ct)
    {
        _hubContext.Clients.All.Hueta(new("Говно жопа хуй"), ct);
        return Task.FromResult(new Unit());
    }
}