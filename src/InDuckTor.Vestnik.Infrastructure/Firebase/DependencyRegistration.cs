using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

public class FirebaseConfiguration
{
    public required string ProjectId { get; set; }
    public required string CredentialsFilePath { get; set; }
}

public static class DependencyRegistration
{
    /// <param name="services"></param>
    /// <param name="configuration">Секция состоящая из ключей - id проектов в firebase и значений - <see cref="FirebaseConfiguration"/></param>
    /// <returns></returns>
    public static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInDuckTorBankFirebaseApp(configuration.GetSection(InDuckTorBankFirebaseApp.Name));

        return services;
    }

    public static IServiceCollection AddInDuckTorBankFirebaseApp(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FirebaseConfiguration>(name: InDuckTorBankFirebaseApp.Name, configuration);

        services
            .AddSingleton<InDuckTorBankFirebaseApp>()
            .AddKeyedSingleton <IMessageSender, FirebaseMessageSender>(Domain.ApplicationVariant.ClientBank)
            //todo .AddKeyedSingleton <IInDuckTorBankMessageSender, >(Domain.ApplicationVariant.EmployeeWebInterface)
            ;

        return services;
    }
}