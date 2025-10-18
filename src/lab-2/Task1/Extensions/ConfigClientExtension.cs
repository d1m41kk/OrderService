using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Task1.Interfaces;

namespace Task1.Extensions;

public static class ConfigClientExtension
{
    public static IHttpClientFactory AddConfigClientHttp(this IServiceCollection services, IConfiguration configuration)
    {
        string baseUrl = configuration["Api:BaseAddress"] ?? "http://localhost:8080/";
        services.AddHttpClient("client", client =>
            client.BaseAddress = new Uri(baseUrl));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IHttpClientFactory>();
    }

    public static IServiceCollection AddConfigClientRefit(this IServiceCollection services, IConfiguration configuration)
    {
        string baseUrl = configuration["Api:BaseAddress"] ?? "http://localhost:8080/";
        services.AddRefitClient<IRefitClientConfigurationServiceApi>().ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUrl));
        return services;
    }
}