using Microsoft.AspNetCore.SignalR;

namespace InDuckTor.Vestnik.Features.Account;

public static class AccountGroupExtensions
{
    public static Task AddToAccountGroup(
        this IGroupManager groupManager,
        string connectionId,
        string accountNumber,
        CancellationToken cancellationToken)
    {
        return groupManager.AddToGroupAsync(
            connectionId,
            AccountGroup.GetAccountGroupName(accountNumber),
            cancellationToken);
    }
}

public static class AccountGroup
{
    private const string AccountGroupName = "account/";

    public static string GetAccountGroupName(string accountNumber) => AccountGroupName + accountNumber;
}