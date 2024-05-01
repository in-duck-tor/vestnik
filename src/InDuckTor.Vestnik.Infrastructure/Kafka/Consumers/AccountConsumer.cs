using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Interceptors;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Kafka.Interceptors;
using InDuckTor.Shared.Strategies;
using InDuckTor.Shared.Utils;
using InDuckTor.Vestnik.Domain;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Kafka.Consumers;

[RetryStrategyStatic(3, retryDelaysSeconds: [ 0, 1, 1 ])]
[Intercept(typeof(ConversationConsumerInterceptor<Null, AccountEnvelop>))] // todo remove 
public class AccountConsumer(
    IMulticastExecutor multicastExecutor,
    ILogger<AccountConsumer> logger) : IConsumerStrategy<Null, AccountEnvelop>
{
    public async Task<ProcessingResult> Execute(ConsumeResult<Null, AccountEnvelop> message, CancellationToken cancellationToken)
    {
        var envelop = message.Message.Value;
        object? domainMessage = envelop.PayloadCase switch
        {
            AccountEnvelop.PayloadOneofCase.AccountCreated
                => new AccountCreatedEvent(
                    envelop.AccountCreated.AccountNumber,
                    envelop.AccountCreated.Type.GetEnumMemberName(),
                    envelop.AccountCreated.State.GetEnumMemberName(),
                    envelop.AccountCreated.OwnerId,
                    envelop.AccountCreated.CreatedById,
                    envelop.AccountCreated.GrantedUsers.Select(user => (
                            user.Id,
                            user.Actions.Select(action => action.GetEnumMemberName()).ToArray()))
                        .ToArray()),
            AccountEnvelop.PayloadOneofCase.AccountStateChanged
                => new AccountUpdatedEvent(
                    envelop.AccountStateChanged.AccountNumber,
                    envelop.AccountStateChanged.Type.GetEnumMemberName(),
                    envelop.AccountStateChanged.State.GetEnumMemberName(),
                    envelop.AccountStateChanged.ChangedById),
            _ => null
        };

        if (domainMessage is null)
        {
            logger.LogDebug("Получено событие {EventType} неизвестного типа {PayloadCase}",
                typeof(AccountEnvelop),
                message.Message.Value.PayloadCase);
            return ProcessingResult.Skip;
        }

        var result = await multicastExecutor.Execute(domainMessage, cancellationToken);
        if (result.IsFailed)
        {
            logger.LogWarning("Событие обработано с ошибкой : {Result}", result);
        }

        return ProcessingResult.Ok;
    }
}