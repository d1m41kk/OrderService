using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Task1.Interfaces;
using Task1.Models;

namespace Task2.Implementations;

public class CustomConfigurationService : BackgroundService
{
    private readonly CustomConfigurationProvider _provider;
    private readonly IConfigurationServiceClient _client;
    private readonly CustomConfigurationServiceOptions _options;

    public CustomConfigurationService(
        CustomConfigurationProvider provider,
        IConfigurationServiceClient client,
        IOptions<CustomConfigurationServiceOptions> options)
    {
        _provider = provider;
        _client = client;
        _options = options.Value;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await UpdateOnceAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    public async Task UpdateOnceAsync(CancellationToken token)
    {
        var allItems = new List<ConfigurationItemDto>();

        await foreach (QueryConfigurationsResponse response in _client.GetAllConfigsAsync(token))
        {
            allItems.AddRange(response.Items);
        }

        _provider.DoReload(allItems);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.RefreshInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateOnceAsync(stoppingToken);
        }
    }
}