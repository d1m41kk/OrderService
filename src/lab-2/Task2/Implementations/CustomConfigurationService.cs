using Microsoft.Extensions.Hosting;
using Task1.Interfaces;
using Task1.Models;

namespace Task2.Implementations;

public class CustomConfigurationService : BackgroundService
{
    private readonly CustomConfigurationProvider _provider;
    private readonly IConfigurationServiceClient _client;
    private readonly PeriodicTimer _timer;
    private readonly int _pageSize;

    public CustomConfigurationService(CustomConfigurationProvider provider, IConfigurationServiceClient client, PeriodicTimer timer, int pageSize)
    {
        _provider = provider;
        _client = client;
        _timer = timer;
        _pageSize = pageSize;
    }

    public async Task UpdateOnceAsync(CancellationToken token)
    {
        {
            var allItems = new List<ConfigurationItemDto>();

            await foreach (QueryConfigurationsResponse response in _client.GetAllConfigsAsync(_pageSize, null, token))
            {
                allItems.AddRange(response.Items);
            }

            var configs = new QueryConfigurationsResponse(allItems, null);
            _provider.DoReload(configs);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await UpdateOnceAsync(stoppingToken);
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await UpdateOnceAsync(stoppingToken);
        }
    }
}