using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Vestnik.Infrastructure.Database;

public static class DependenciesRegistration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<VestnikDbContext>((provider, builder) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("VestnikDatabase");
            ArgumentException.ThrowIfNullOrEmpty(connectionString);

            builder.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}