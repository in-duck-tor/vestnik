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
            AccountGroup.GetGroupName(accountNumber),
            cancellationToken);
    }

    public static Task AddToTransactionGroup(
        this IGroupManager groupManager,
        string connectionId,
        long transactionId,
        CancellationToken cancellationToken)
    {
        return groupManager.AddToGroupAsync(
            connectionId,
            TransactionGroup.GetGroupName(transactionId),
            cancellationToken);
    }
}

public static class AccountGroup
{
    private const string AccountGroupName = "account/";

    public static string GetGroupName(string accountNumber) => AccountGroupName + accountNumber;
}

public static class TransactionGroup
{
    private const string TransactionGroupName = "transaction/";

    public static string GetGroupName(long transactionId) => TransactionGroupName + transactionId;
}