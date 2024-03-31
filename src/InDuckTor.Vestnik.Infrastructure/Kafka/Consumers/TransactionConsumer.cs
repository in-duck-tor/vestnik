using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Kafka;

public class TransactionConsumer(ILogger<TransactionConsumer> logger) : ITopicConsumer<string, TransactionEnvelop>
{
    public Task<ProcessingResult> Consume(ConsumeResult<string, TransactionEnvelop> message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}