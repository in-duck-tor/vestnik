using System.Threading.Tasks.Dataflow;
using InDuckTor.Account.HttpClient;
using InDuckTor.Shared.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace InDuckTor.Vestnik.Features.Account;

[SignalRHub]
[Authorize]
public class AccountEventsHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToMyAccounts(
        IExecutor<IUserAccountsQuery, GetUserAccountsArgs, IEnumerable<AccountDto>> getUserAccounts,
        CancellationToken cancellationToken)
    {
        if (int.TryParse(Context.UserIdentifier, out var userId)) return;
        var accountDtos = await getUserAccounts.Execute(new(userId), cancellationToken);

        foreach (var accountDto in accountDtos)
        {
            await Groups.AddToAccountGroup(Context.ConnectionId, accountDto.Number, cancellationToken);
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