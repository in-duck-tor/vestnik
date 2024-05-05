using InDuckTor.Account.HttpClient;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace InDuckTor.Vestnik.Features.Account;

public interface IAccountEventsHub
{
    Task AccountCreated(AccountCreatedEvent @event, CancellationToken cancellationToken);
    Task AccountUpdatedEvent(AccountUpdatedEvent @event, CancellationToken ct);
    Task TransactionCreatedEvent(TransactionCreatedEvent @event, CancellationToken ct);
    Task TransactionUpdatedEvent(TransactionUpdatedEvent @event, CancellationToken ct);
    Task ReceivePleasure();
}

[SignalRHub]
public class AccountEventsHub : Hub<IAccountEventsHub>
{
    [Authorize]
    public async Task SubscribeToMyAccounts(
        IExecutor<IUserAccountsQuery, GetUserAccountsArgs, IEnumerable<AccountDto>> getUserAccounts,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(Context.UserIdentifier, out var userId)) return;
        var accountDtos = await getUserAccounts.Execute(new(userId), cancellationToken);

        foreach (var accountDto in accountDtos)
        {
            if (accountDto.Number == null) continue;
            await Groups.AddToAccountGroup(Context.ConnectionId, accountDto.Number, cancellationToken);
        }
    }

    public async Task SubscribeSosok(string[] sosochki)
    {
        Console.WriteLine("Subscribe Sosok method started");
        foreach (var s in sosochki)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, s);
        }

        Console.WriteLine("Calling client's method");
        await Clients.Groups(sosochki).ReceivePleasure();
    }

    [Authorize]
    public async Task SubscribeAccounts(
        string[] accountsToSubscribe,
        IExecutor<IUserAccountsQuery, GetUserAccountsArgs, IEnumerable<AccountDto>> getUserAccounts,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        if (int.TryParse(Context.UserIdentifier, out var userId)) return;
        var accountDtos = await getUserAccounts.Execute(new(userId), cancellationToken);

        foreach (var accountDto in accountDtos)
        {
            await Groups.AddToAccountGroup(Context.ConnectionId, accountDto.Number, cancellationToken);
        }
    }
}