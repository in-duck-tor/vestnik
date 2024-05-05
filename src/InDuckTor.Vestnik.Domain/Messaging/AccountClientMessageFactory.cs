using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace InDuckTor.Vestnik.Domain.Messaging;

public class AccountClientMessageFactory
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
            .Append("Ты можешь увидеть его в списке своих счётов");

        var notificationFromUpdated = new NotificationDataBase(
            Title: "Состояние счёта обновлено !",
            Body: stringBuilder.ToString()
        );

        StringBuilders.Return(stringBuilder);
        return notificationFromUpdated;
    }

    public static NotificationDataBase NotificationFromCreated(AccountCreatedEvent @event)
        => new(
            Title: "Создан Новый Счёт !",
            Body: $"Счёт {@event.AccountNumber}.\nТы можешь увидеть его в списке своих счётов"
        );
}