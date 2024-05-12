using System.Collections.Concurrent;
using InDuckTor.Account.HttpClient;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Connecting, id: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Disconnecting, id: {Context.ConnectionId}");
        Console.WriteLine(exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToMyAccounts(
        [FromServices] IExecutor<IUserAccountsQuery, GetUserAccountsArgs, IEnumerable<AccountDto>> getUserAccounts)
    {
        if (!int.TryParse(Context.UserIdentifier, out var userId)) return;
        var ct = new CancellationToken();
        var accountDtos = await getUserAccounts.Execute(new(userId), ct);

        foreach (var accountDto in accountDtos)
        {
            if (accountDto.Number == null) continue;
            await Groups.AddToAccountGroup(Context.ConnectionId, accountDto.Number, ct);
        }
    }

    // TODO: проверить роль
    public async Task SubscribeAccounts(IEnumerable<string> accountsToSubscribe)
    {
        var ct = new CancellationToken();
        foreach (var account in accountsToSubscribe)
        {
            await Groups.AddToAccountGroup(Context.ConnectionId, account, ct);
        }
    }
}