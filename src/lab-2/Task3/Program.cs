using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task1.Extensions;
using Task1.Implementations;
using Task1.Interfaces;
using Task2.Implementations;
using Task3.Models;
using Task3.Services;

var configurationBuilder = new ConfigurationBuilder();
var customProvider = new CustomConfigurationProvider();
configurationBuilder.Add(new CustomConfigurationProviderSource(customProvider));
IConfiguration configuration = configurationBuilder.Build();

var services = new ServiceCollection();
services.AddSingleton(configuration);
services.AddSingleton(customProvider);

services.Configure<DisplayInfo>(configuration.GetSection("Display"));

services.AddConfigClientRefit();
services.AddTransient<IConfigsClient, ConfigClientRefit>();

services.AddSingleton<Renderer>();
services.AddSingleton<DisplayService>();

services.AddSingleton(sp =>
{
    CustomConfigurationProvider provider = sp.GetRequiredService<CustomConfigurationProvider>();
    IConfigsClient client = sp.GetRequiredService<IConfigsClient>();
    var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
    int pageSize = 1;
    return new CustomConfigurationService(provider, client, timer, pageSize);
});

ServiceProvider sp = services.BuildServiceProvider();

sp.GetRequiredService<CustomConfigurationService>().StartUpdating();
sp.GetRequiredService<DisplayService>().StartRender();

Console.WriteLine("Running without Generic Host. Press Ctrl+C to exit.");
var done = new ManualResetEventSlim(false);
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    done.Set();
};
done.Wait();