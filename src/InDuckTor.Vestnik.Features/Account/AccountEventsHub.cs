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
[Authorize(AuthenticationSchemes = "Signalr")]
public class AccountEventsHub : Hub<IAccountEventsHub>
{
    public static readonly ConcurrentDictionary<long, IList<string>> TransactionToAccounts = new();

    private readonly IExecutor<IUserAccountsQuery, GetUserAccountsArgs, IEnumerable<AccountDto>> _getUserAccounts;

    public AccountEventsHub(IExecutor<IUserAccountsQuery, GetUserAccountsArgs, IEnumerable<AccountDto>> getUserAccounts)
    {
        _getUserAccounts = getUserAccounts;
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Connecting, id: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Disconnecting, id: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToMyAccounts()
    {
        if (!int.TryParse(Context.UserIdentifier, out var userId)) return;
        var ct = new CancellationToken();
        var accountDtos = await _getUserAccounts.Execute(new(userId), ct);

        foreach (var accountDto in accountDtos)
        {
            if (accountDto.Number == null) continue;
            await Groups.AddToAccountGroup(Context.ConnectionId, accountDto.Number, ct);
        }
    }

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