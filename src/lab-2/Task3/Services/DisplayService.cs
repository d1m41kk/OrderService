using Microsoft.Extensions.Options;
using Task3.Models;

namespace Task3.Services;

public class DisplayService : IDisposable
{
    private readonly Renderer _renderer;
    private readonly IOptionsMonitor<DisplayInfo> _displayInfoMonitor;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private PeriodicTimer _renderTimer;
    private Task? _renderTask;

    public DisplayService(Renderer renderer,  IOptionsMonitor<DisplayInfo> displayInfoMonitor)
    {
        _renderer = renderer;
        _displayInfoMonitor = displayInfoMonitor;
        _renderTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(displayInfoMonitor.CurrentValue.UpdateTime));
        _cancellationTokenSource = new CancellationTokenSource();
        _displayInfoMonitor.OnChange(OnConfigurationChanged);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _renderTimer.Dispose();
        _renderTask?.Dispose();
    }

    public void StartRender()
    {
        if (_renderTask != null)
        {
            return;
        }

        _renderTask = RenderLoop(_cancellationTokenSource.Token);
    }

    public void StopRender()
    {
        _cancellationTokenSource.Cancel();
        _renderTimer.Dispose();
    }

    private void OnConfigurationChanged(DisplayInfo newInfo, string? s)
    {
        _renderTimer.Dispose();
        _renderTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(newInfo.UpdateTime));
        _displayInfoMonitor.CurrentValue.UpdateTime = newInfo.UpdateTime;

        _ = RenderOnce(_cancellationTokenSource.Token);
    }

    private async Task RenderOnce(CancellationToken token)
    {
        await _renderer.Render(_displayInfoMonitor.CurrentValue, token);
    }

    private async Task RenderLoop(CancellationToken token)
    {
        await RenderOnce(token);
        while (await _renderTimer.WaitForNextTickAsync(token))
        {
            await RenderOnce(token);
        }
    }
}