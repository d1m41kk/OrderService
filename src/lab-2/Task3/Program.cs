using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task2.Implementations;
using Task3;
using Task3.Services;

IHost host = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        var configurationBuilder = new ConfigurationBuilder();
        var customProvider = new CustomConfigurationProvider();
        configurationBuilder.Add(new CustomConfigurationProviderSource(customProvider));
        IConfiguration configuration = configurationBuilder.Build();

        services.InjectServices(configuration, customProvider);
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