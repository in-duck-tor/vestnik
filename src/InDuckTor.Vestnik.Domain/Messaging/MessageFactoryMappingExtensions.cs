using InDuckTor.Account.Contracts.Public;

namespace InDuckTor.Vestnik.Domain.Messaging;

public static class MessageFactoryMappingExtensions
{
    /// <returns>Имя статуса для отображения или <c>null</c> если значение не известно</returns>
    public static string? ToViewStyle(this AccountState accountState)
        => accountState switch
        {
            AccountState.Active => "Активен",
            AccountState.Closed => "Закрыт",
            AccountState.Frozen => "Заморожен",
            AccountState.Unknown or _ => null
        };

    /// <returns>Имя типа для отображения или <c>null</c> если значение не известно</returns>
    public static string ToViewStyle(this TransactionType transactionType)
        => transactionType switch
        {
            TransactionType.Withdraw => "Снятие",
            TransactionType.Deposit or TransactionType.TransferFromExternal => "Пополнение",
            TransactionType.Transfer or TransactionType.TransferToExternal => "Перевод",
            TransactionType.Unknown or _ => null
        };
}