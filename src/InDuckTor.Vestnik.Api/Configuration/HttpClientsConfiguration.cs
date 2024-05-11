using InDuckTor.Account.HttpClient;
using InDuckTor.Vestnik.Api.Services;
using Microsoft.Extensions.Options;

namespace InDuckTor.Vestnik.Api.Configuration;

public class HttpClientConfiguration
{
    public required Uri BaseUrl { get; set; }
}

public static class HttpClientsConfiguration
{
    /// <summary>
    /// Настраивает http клиенты для сервиса
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">Секция конфигурации для http клиентов</param>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<InDuckTorSystemTokenHandler>();

        services.Configure<HttpClientConfiguration>(configuration.GetSection("Account"))
            .AddHttpClient<IAccountClient, AccountClient>((client, provider) =>
            {
                var config = provider.GetRequiredService<IOptionsSnapshot<HttpClientConfiguration>>();
                return new(config.Value.BaseUrl.ToString(), client);
                // return new(client);
            })
            .AddHttpMessageHandler<InDuckTorSystemTokenHandler>();

        return services;
    }
}