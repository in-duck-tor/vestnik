using Microsoft.AspNetCore.SignalR;

namespace InDuckTor.Vestnik.Features.Account;

public static class GroupManagerExtensions
{
    public static Task AddToAccountGroup(this IGroupManager groupManager, string connectionId, string accountNumber, CancellationToken cancellationToken)
    {
        return groupManager.AddToGroupAsync(connectionId, $"account/{accountNumber}", cancellationToken);
    }
}