using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace InDuckTor.Vestnik.Domain.Messaging;

public class AccountEmployeeMessageFactory
{
    private static readonly ObjectPool<StringBuilder> StringBuilders = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

    public static NotificationDataBase NotificationFromUpdated(AccountUpdatedEvent @event)
    {
        var stringBuilder = StringBuilders.Get();

        stringBuilder.Append("Счёт ").Append(@event.AccountNumber);
        var accountStateName = @event.AccountState.ToViewStyle();
        if (!string.IsNullOrEmpty(accountStateName))
        {
            stringBuilder.Append(" в состоянии ").Append(accountStateName);
        }

        stringBuilder.AppendLine(".")
            .Append("Отредактировано пользователем : ").Append(@event.ChangedById);

        var notificationFromUpdated = new NotificationDataBase(
            Title: "Состояние счёта обновлено !",
            Body: stringBuilder.ToString()
        );

        StringBuilders.Return(stringBuilder);
        return notificationFromUpdated;
    }

    public static NotificationDataBase NotificationFromCreated(AccountCreatedEvent @event)
    {
        var stringBuilder = StringBuilders.Get();

        stringBuilder.Append("Счёт ").AppendLine(@event.AccountNumber)
            .Append("Создан пользователем : ").AppendLine(@event.CreatedById.ToString())
            .Append("Для пользователя : ").AppendLine(@event.OwnerId.ToString());

        var notificationFromCreated = new NotificationDataBase(
            Title: "Создан Новый Счёт !",
            Body: stringBuilder.ToString()
        );

        StringBuilders.Return(stringBuilder);
        return notificationFromCreated;
    }
}