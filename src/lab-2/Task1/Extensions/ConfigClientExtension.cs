using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Task1.Interfaces;

namespace Task1.Extensions;

public static class ConfigClientExtension
{
    public static IServiceCollection AddConfigClientHttp(this IServiceCollection services)
    {
        services.AddHttpClient("client", (serviceProvider, client) =>
        {
            IOptions<ApiOptions> options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseAddress);
        });
        return services;
    }

    public static IServiceCollection AddConfigClientRefit(this IServiceCollection services)
    {
        services.AddRefitClient<IRefitClientConfigurationServiceApi>().ConfigureHttpClient((serviceProvider, client) =>
        {
            IOptions<ApiOptions> options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseAddress);
        });
        return services;
    }
}