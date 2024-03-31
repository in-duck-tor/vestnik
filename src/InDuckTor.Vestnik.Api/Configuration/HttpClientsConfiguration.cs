using InDuckTor.Account.HttpClient;
using InDuckTor.Vestnik.Api.Services;
using Microsoft.Extensions.Options;

namespace InDuckTor.Vestnik.Api.Configuration;

public static class HttpClientsConfiguration
{
    public record HttpClientConfiguration(Uri BaseUrl);

    /// <summary>
    /// Настраивает http клиенты для сервиса
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">Секция конфигурации для http клиентов</param>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HttpClientConfiguration>(configuration.GetSection("Account"))
            .AddHttpClient<IAccountClient, AccountClient>((client, provider) =>
            {
                var config = provider.GetRequiredService<IOptionsSnapshot<HttpClientConfiguration>>();
                return new AccountClient(config.Value.BaseUrl.ToString(), client);
            })
            .AddHttpMessageHandler<InDuckTorSystemTokenHandler>();

        return services;
    }
}