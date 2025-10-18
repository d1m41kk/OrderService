using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task1.Extensions;
using Task1.Implementations;
using Task1.Interfaces;
using Task2.Implementations;
using Task3.Models;
using Task3.Services;

namespace Task3;

public static class ServiceCollectionExtension
{
    public static void InjectServices(this IServiceCollection services, IConfiguration configuration, CustomConfigurationProvider provider)
    {
        services.AddSingleton(configuration);
        services.AddSingleton(provider);

        services.Configure<DisplayInfo>(configuration.GetSection("Display"));
        services.Configure<ApiOptions>(configuration.GetSection("Api"));
        services.Configure<CustomConfigurationServiceOptions>(options =>
        {
            options.PageSize = 1;
            options.RefreshInterval = TimeSpan.FromSeconds(1);
        });

        services.AddConfigClientRefit();
        services.AddTransient<IConfigurationServiceClient, RefitClientConfigurationService>();
        services.AddSingleton<Renderer>();
        services.AddSingleton<DisplayService>();
        services.AddHostedService<CustomConfigurationService>();
    }
}