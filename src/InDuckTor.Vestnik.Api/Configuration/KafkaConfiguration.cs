using Confluent.Kafka;
using InDuckTor.Shared.Kafka;

namespace InDuckTor.Vestnik.Api.Configuration;

public static class KafkaConfiguration
{
    public static IServiceCollection AddVestnikKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var schemaRegistries = configuration.GetSection("SchemaRegistries");
        var defaultSchemaRegistry = schemaRegistries.GetSection("Default");

        var consumers = configuration.GetSection("Consumers");
        var producers = configuration.GetSection("Producers");

        var accountsSection = consumers.GetSection("Accounts");

        services.AddInDuckTorKafka()
            .AddConsumer<AccountConsumer, Null, string>(accountsSection, config => { });

        return services;
    }
}

public class AccountConsumer : ITopicConsumer<Null, string>
{
    public Task<ProcessingResult> Consume(ConsumeResult<Null, string> message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}