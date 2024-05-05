using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace InDuckTor.Vestnik.Domain.Messaging;

public class TransactionEmployeeMessageFactory
{
    private static readonly ObjectPool<StringBuilder> StringBuilders = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

    public static NotificationDataBase CreateNotificationFromStarted(TransactionCreatedEvent @event)
    {
        var stringBuilder = StringBuilders.Get();

        stringBuilder.Append("Трансакция с типом ").Append(@event.Type.ToViewStyle());
        if (@event.DepositOn != null)
        {
            stringBuilder.AppendLine("Зачислено на счёт ").Append(@event.DepositOn.AccountNumber);
        }
        if (@event.WithdrawFrom != null)
        {
            stringBuilder.AppendLine("Снятие со счёта ").Append(@event.WithdrawFrom.AccountNumber);
        }
        
        var notification = new NotificationDataBase(
            Title: "Создана новая трансакция !",
            Body: stringBuilder.ToString());
        
        StringBuilders.Return(stringBuilder);
        return notification;
    }
}