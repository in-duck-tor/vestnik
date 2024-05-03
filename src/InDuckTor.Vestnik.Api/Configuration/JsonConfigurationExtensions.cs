using System.Text.Json.Serialization;
using InDuckTor.Vestnik.Domain;
using Microsoft.AspNetCore.Http.Json;

namespace InDuckTor.Vestnik.Api.Configuration;

public static class JsonConfigurationExtensions
{
    public static IServiceCollection ConfigureJsonConverters(this IServiceCollection serviceCollection) => serviceCollection.Configure<JsonOptions>(ConfigureJsonOptions);

    private static void ConfigureJsonOptions(JsonOptions options)
    {
        var jsonConverters = options.SerializerOptions.Converters;
        
        var enumMemberConverter = new JsonStringEnumMemberConverter(
            new JsonStringEnumMemberConverterOptions(),
            typeof(ApplicationVariant));
        jsonConverters.Add(enumMemberConverter);
    }   
}