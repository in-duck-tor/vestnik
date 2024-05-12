using System.Text;
using InDuckTor.Account.Contracts.Public;
using Microsoft.Extensions.ObjectPool;

namespace InDuckTor.Vestnik.Domain.Messaging;

public class TransactionClientMessageFactory
{
    // todo may be use common pool between all factories ?
    private static readonly ObjectPool<StringBuilder> StringBuilders = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

    public static IEnumerable<(int userId, NotificationDataBase notification)> NotificationsFromStarted(TransactionCreatedEvent @event)
    {
        switch (@event.Type)
        {
            case TransactionType.TransferToExternal when @event is { WithdrawFrom.AccountOwnerId: not null }:
            case TransactionType.Withdraw when @event is { WithdrawFrom.AccountOwnerId: not null }:
                yield return (@event.WithdrawFrom.AccountOwnerId.Value, CreateFundsRemovalNotification(@event.WithdrawFrom));
                break;

            case TransactionType.TransferFromExternal when @event is { DepositOn.AccountOwnerId: not null }:
            case TransactionType.Deposit when @event is { DepositOn.AccountOwnerId: not null }:
                yield return (@event.DepositOn.AccountOwnerId.Value, CreateFundsAdditionNotification(@event.DepositOn));
                break;

            case TransactionType.Transfer when @event is { DepositOn.AccountOwnerId: not null, WithdrawFrom.AccountOwnerId: not null }:
                if (@event.DepositOn.AccountOwnerId == @event.WithdrawFrom.AccountOwnerId)
                {
                    yield return (@event.DepositOn.AccountOwnerId.Value, CreateOwnAccountsTransferNotification(@event));
                }
                else
                {
                    yield return (@event.DepositOn.AccountOwnerId.Value, CreateFundsAdditionNotification(@event.DepositOn));
                    yield return (@event.WithdrawFrom.AccountOwnerId.Value, CreateFundsRemovalNotification(@event.WithdrawFrom));
                }

                break;

            case TransactionType.Unknown:
            default:
                break; // skip
        }
    }

    public static NotificationDataBase CreateFundsAdditionNotification(TransactionTarget depositOn)
    {
        var stringBuilder = StringBuilders.Get();

        stringBuilder.Append("Ваш счёт ").Append(depositOn.AccountNumber)
            .Append(" пополнен на ")
            .Append(depositOn.Amount).Append(' ').Append(depositOn.CurrencyCode);

        var notification = new NotificationDataBase(
            Title: "Пополнение средств !",
            Body: stringBuilder.ToString());

        StringBuilders.Return(stringBuilder);
        return notification;
    }

    public static NotificationDataBase CreateFundsRemovalNotification(TransactionTarget withdrawFrom)
    {
        var stringBuilder = StringBuilders.Get();

        stringBuilder.Append("С вашего счёта ").Append(withdrawFrom.AccountNumber)
            .Append("снято ")
            .Append(withdrawFrom.Amount).Append(' ').Append(withdrawFrom.CurrencyCode);

        var notification = new NotificationDataBase(
            Title: "Снятие средств !",
            Body: stringBuilder.ToString());

        StringBuilders.Return(stringBuilder);
        return notification;
    }

    public static NotificationDataBase CreateOwnAccountsTransferNotification(TransactionCreatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event.WithdrawFrom);
        ArgumentNullException.ThrowIfNull(@event.DepositOn);
        
        var stringBuilder = StringBuilders.Get();

        stringBuilder.Append("Переведено с ").Append(@event.WithdrawFrom.AccountNumber)
            .Append(" на ").Append(@event.DepositOn.AccountNumber)
            .Append(' ').Append(@event.WithdrawFrom.Amount).Append(' ').Append(@event.WithdrawFrom.CurrencyCode);

        var notification = new NotificationDataBase(
            Title: "Перевод между вашими счётами !",
            Body: stringBuilder.ToString());

        StringBuilders.Return(stringBuilder);
        return notification;
    }
}