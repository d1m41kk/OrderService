using Task1.Interfaces;
using Task1.Models;

namespace Task2.Implementations;

public class CustomConfigurationService : IDisposable
{
    private readonly CustomConfigurationProvider _provider;
    private readonly IConfigsClient _client;
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private readonly int _pageSize;
    private Task? _taskLooping;

    public CustomConfigurationService(CustomConfigurationProvider provider, IConfigsClient client, PeriodicTimer timer, int pageSize)
    {
        _provider = provider;
        _client = client;
        _timer = timer;
        _pageSize = pageSize;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _cts.Dispose();
    }

    public void StartUpdating()
    {
        if (_taskLooping != null)
        {
            return;
        }

        _taskLooping = StartUpdatingAsync(_cts.Token);
    }

    public async Task StopUpdating()
    {
        await _cts.CancelAsync();
        if (_taskLooping != null)
        {
            await _taskLooping;
        }
    }

    public async Task UpdateOnceAsync(CancellationToken token)
    {
        {
            var allItems = new List<ConfigurationItemDto>();

            await foreach (QueryConfigurationsResponse? response in _client.GetAllConfigsAsync(_pageSize, null, token))
            {
                if (response?.Items != null)
                {
                    allItems.AddRange(response.Items);
                }
            }

            var configs = new QueryConfigurationsResponse(allItems, null);
            _provider.DoReload(configs);
        }
    }

    private async Task StartUpdatingAsync(CancellationToken token)
    {
        await UpdateOnceAsync(token);
        while (await _timer.WaitForNextTickAsync(token))
        {
            await UpdateOnceAsync(token);
        }
    }
}