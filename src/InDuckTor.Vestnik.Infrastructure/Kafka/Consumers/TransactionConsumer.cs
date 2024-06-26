using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Domain;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Kafka.Consumers;

[RetryStrategyStatic(3, retryDelaysSeconds: [0, 1, 1])]
public class TransactionConsumer(IMulticastExecutor multicastExecutor, ILogger<TransactionConsumer> logger)
    : IConsumerStrategy<string, TransactionEnvelop>
{
    public async Task<ProcessingResult> Execute(ConsumeResult<string, TransactionEnvelop> message,
        CancellationToken cancellationToken)
    {
        var envelop = message.Message.Value;
        object? domainMessage = envelop.PayloadCase switch
        {
            TransactionEnvelop.PayloadOneofCase.TransactionStarted
                => new TransactionCreatedEvent(
                    envelop.TransactionStarted.Id,
                    envelop.TransactionStarted.Type,
                    envelop.TransactionStarted.Status,
                    MapTransactionTarget(envelop.TransactionStarted.DepositOn),
                    MapTransactionTarget(envelop.TransactionStarted.WithdrawFrom)),
            TransactionEnvelop.PayloadOneofCase.TransactionFinished
                => new TransactionUpdatedEvent(
                    envelop.TransactionFinished.Id,
                    envelop.TransactionFinished.Type,
                    envelop.TransactionFinished.Status),
            _ => null
        };

        if (domainMessage is null)
        {
            logger.LogDebug("Получено событие {EventType} неизвестного типа {PayloadCase}", typeof(AccountEnvelop),
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

     private static TransactionTarget? MapTransactionTarget(TransactionStarted.Types.Target? target) => target == null
        ? null
        : new(target.AccountNumber, target.CurrencyCode, target.Amount, target.BankCode, target.AccountOwnerId);
}