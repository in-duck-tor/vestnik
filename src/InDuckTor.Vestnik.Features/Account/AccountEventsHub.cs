using System.Collections.Concurrent;
using InDuckTor.Account.HttpClient;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace InDuckTor.Vestnik.Features.Account;

public record RandomShit(string Message);

public interface IAccountEventsHub
{
    Task AccountCreated(AccountCreatedEvent @event, CancellationToken cancellationToken);
    Task AccountUpdated(AccountUpdatedEvent @event, CancellationToken ct);
    Task TransactionCreated(TransactionCreatedEvent @event, CancellationToken ct);
    Task TransactionUpdated(TransactionUpdatedEvent @event, CancellationToken ct);
    Task Hueta(RandomShit @event, CancellationToken ct);
}

[SignalRHub]
public class AccountEventsHub : Hub<IAccountEventsHub>
{
    public static readonly ConcurrentDictionary<long, IList<string>> TransactionToAccounts = new();

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