using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;
using Microsoft.Extensions.Logging;

namespace InDuckTor.Vestnik.Infrastructure.Kafka;

public class AccountConsumer(ILogger<AccountConsumer> logger) : ITopicConsumer<Null, AccountEnvelop>
{
    public Task<ProcessingResult> Consume(ConsumeResult<Null, AccountEnvelop> message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}