using Microsoft.Extensions.DependencyInjection;
using Refit;
using Task1.Interfaces;

namespace Task1.Extensions;

public static class ConfigClientExtension
{
    public static IHttpClientFactory AddConfigClientHttp(this IServiceCollection services)
    {
        services.AddHttpClient("client", client =>
            client.BaseAddress = new Uri("http://localhost:8080/"));

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IHttpClientFactory>();
    }

    public static IServiceCollection AddConfigClientRefit(this IServiceCollection services)
    {
        services.AddRefitClient<IApi>().ConfigureHttpClient(client => client.BaseAddress = new Uri("http://localhost:8080"));
        return services;
    }
}