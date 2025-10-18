using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task1.Extensions;
using Task1.Implementations;
using Task1.Interfaces;
using Task2.Implementations;
using Task3.Models;
using Task3.Services;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        var configurationBuilder = new ConfigurationBuilder();
        var customProvider = new CustomConfigurationProvider();
        configurationBuilder.Add(new CustomConfigurationProviderSource(customProvider));
        IConfiguration configuration = configurationBuilder.Build();

        services.AddSingleton(configuration);
        services.AddSingleton(customProvider);
        services.Configure<DisplayInfo>(configuration.GetSection("Display"));
        services.AddConfigClientRefit(configuration);
        services.AddTransient<IConfigurationServiceClient, RefitClientConfigurationService>();
        services.AddSingleton<Renderer>();
        services.AddSingleton<DisplayService>();

        services.AddHostedService(sp =>
        {
            CustomConfigurationProvider provider = sp.GetRequiredService<CustomConfigurationProvider>();
            IConfigurationServiceClient client = sp.GetRequiredService<IConfigurationServiceClient>();
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            int pageSize = 1;
            return new CustomConfigurationService(provider, client, timer, pageSize);
        });
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
    })
    .UseConsoleLifetime()
    .Build();

DisplayService displayService = host.Services.GetRequiredService<DisplayService>();
displayService.StartRender();

Console.WriteLine("Application is running. Press Ctrl+C to exit.");

await host.RunAsync();