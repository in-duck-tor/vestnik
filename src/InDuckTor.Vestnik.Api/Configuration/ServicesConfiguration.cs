using InDuckTor.Vestnik.Features.Messaging.Services;

namespace InDuckTor.Vestnik.Api.Configuration;

public static class ServicesConfiguration
{
    public static IServiceCollection AddVestnikServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageSenderService, MessageSenderService>();

        return services;
    } 
}