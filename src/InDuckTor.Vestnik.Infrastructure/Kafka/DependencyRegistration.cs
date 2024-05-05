using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;
using InDuckTor.Vestnik.Infrastructure.Kafka.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Vestnik.Infrastructure.Kafka;

public static class DependencyRegistration
{
    public static IServiceCollection AddVestnikKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var schemaRegistries = configuration.GetSection("SchemaRegistries");
        var defaultSchemaRegistry = schemaRegistries.GetSection("Default");

        var consumers = configuration.GetSection("Consumers");
        var producers = configuration.GetSection("Producers");

        services.AddScoped<AccountConsumer>();
        services.AddScoped<TransactionConsumer>();
        
        services.AddInDuckTorKafka()
            .AddConsumer<AccountConsumer, Null, AccountEnvelop>(
                consumers.GetSection("Account"),
                config =>
                {
                    config.AutoOffsetReset = AutoOffsetReset.Latest;
                    // config.GroupId = "";
                },
                builder =>
                {
                    builder.SetValueDeserializer(new ProtobufDeserializer<AccountEnvelop>().AsSyncOverAsync());
                })
            .AddConsumer<TransactionConsumer, string, TransactionEnvelop>(
                consumers.GetSection("AccountTransaction"),
                config =>
                {
                    config.AutoOffsetReset = AutoOffsetReset.Latest;
                },
                builder =>
                {
                    builder.SetValueDeserializer(new ProtobufDeserializer<TransactionEnvelop>().AsSyncOverAsync());
                });

        return services;
    }
}

