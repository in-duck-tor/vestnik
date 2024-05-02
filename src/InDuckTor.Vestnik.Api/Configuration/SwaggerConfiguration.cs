using InDuckTor.Shared.Configuration.Swagger;
using Microsoft.OpenApi.Models;

namespace InDuckTor.Vestnik.Api.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddVestnikSwaggerGen(this IServiceCollection services)
    {
        return services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Vestnik API v1", Version = "v1" });
            options.ConfigureJwtAuth();
            options.AddSignalRSwaggerGen();
        });
    }
}