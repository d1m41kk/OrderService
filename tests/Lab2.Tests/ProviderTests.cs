using Microsoft.Extensions.Options;
using NSubstitute;
using Task1.Interfaces;
using Task1.Models;
using Task2.Implementations;
using Xunit;

namespace Lab2.Tests;

public class ProviderTests : IDisposable
{
    private readonly IConfigurationServiceClient _mockClient;
    private readonly CustomConfigurationProvider _provider;
    private readonly PeriodicTimer _timer;
    private readonly CustomConfigurationService _service;

    public ProviderTests()
    {
        _mockClient = Substitute.For<IConfigurationServiceClient>();
        _provider = new CustomConfigurationProvider();
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        IOptions<CustomConfigurationServiceOptions> options = Options.Create(new CustomConfigurationServiceOptions
        {
            PageSize = 1,
            RefreshInterval = TimeSpan.FromMinutes(1),
        });
        _service = new CustomConfigurationService(_provider, _mockClient, options);

        _provider.Load();
    }

    public void Dispose()
    {
        _service.Dispose();
        _timer.Dispose();
    }

    [Fact]
    public async Task ProviderHasNoKeys_WhenAddItem_ShouldAddAndReload()
    {
        // Arrange
        var newItem = new ConfigurationItemDto("key1", "value1");
        var response = new QueryConfigurationsResponse([newItem], null);

        SetupClientResponse(response);

        // Act
        await _service.UpdateOnceAsync(CancellationToken.None);

        // Assert
        var keys = _provider.GetChildKeys([], null).ToList();
        Assert.Single(keys);

        _provider.TryGet("key1", out string? value);
        Assert.Equal("value1", value);
    }

    [Fact]
    public async Task ProviderHasKey_WhenAddSameItem_ShouldNotReload()
    {
        // Arrange
        var existingItem = new ConfigurationItemDto("key1", "value1");
        var response = new QueryConfigurationsResponse([existingItem], null);

        bool initialReloadResult = _provider.DoReload([existingItem]);
        Assert.True(initialReloadResult);

        _provider.TryGet("key1", out string? initialValue);

        SetupClientResponse(response);

        // Act
        await _service.UpdateOnceAsync(CancellationToken.None);

        // Assert
        var keys = _provider.GetChildKeys([], null).ToList();
        Assert.Single(keys);

        _provider.TryGet("key1", out string? currentValue);
        Assert.Equal("value1", currentValue);
        Assert.Equal(initialValue, currentValue);
    }

    [Fact]
    public async Task ProviderHasKey_WhenUpdateValue_ShouldUpdateAndReload()
    {
        // Arrange
        var existingItem = new ConfigurationItemDto("key1", "value1");
        var updatedItem = new ConfigurationItemDto("key1", "value2");
        var updatedResponse = new QueryConfigurationsResponse([updatedItem], null);

        _provider.DoReload([existingItem]);
        _provider.TryGet("key1", out string? initialValue);
        Assert.Equal("value1", initialValue);

        SetupClientResponse(updatedResponse);

        // Act
        await _service.UpdateOnceAsync(CancellationToken.None);

        // Assert
        _provider.TryGet("key1", out string? updatedValue);
        Assert.Equal("value2", updatedValue);
        Assert.NotEqual(initialValue, updatedValue);
    }

    [Fact]
    public async Task ProviderHasKey_WhenReceiveEmptyCollection_ShouldBecomeEmptyAndReload()
    {
        // Arrange
        var existingItem = new ConfigurationItemDto("key1", "value1");
        var emptyResponse = new QueryConfigurationsResponse([], null);

        _provider.DoReload([existingItem]);
        var keysBefore = _provider.GetChildKeys([], null).ToList();
        Assert.Single(keysBefore);

        SetupClientResponse(emptyResponse);

        // Act
        await _service.UpdateOnceAsync(CancellationToken.None);

        // Assert
        var keysAfter = _provider.GetChildKeys([], null).ToList();
        Assert.Empty(keysAfter);

        _provider.TryGet("key1", out string? value);
        Assert.Null(value);
    }

    [Fact]
    public async Task Provider_WhenMultiplePages_ShouldCombineAllItems()
    {
        // Arrange
        var page1 = new QueryConfigurationsResponse([new ConfigurationItemDto("key1", "value1")], "token1");
        var page2 = new QueryConfigurationsResponse([new ConfigurationItemDto("key2", "value2")], null);

        _mockClient.GetAllConfigsAsync(Arg.Any<CancellationToken>())
            .Returns(CreateAsyncEnumerable(page1, page2));

        // Act
        await _service.UpdateOnceAsync(CancellationToken.None);

        // Assert
        var keys = _provider.GetChildKeys([], null).ToList();
        Assert.Equal(2, keys.Count);

        _provider.TryGet("key1", out string? value1);
        _provider.TryGet("key2", out string? value2);
        Assert.Equal("value1", value1);
        Assert.Equal("value2", value2);
    }

    private void SetupClientResponse(QueryConfigurationsResponse response)
    {
        _mockClient.GetAllConfigsAsync(Arg.Any<CancellationToken>())
            .Returns(CreateAsyncEnumerable(response));
    }

    private async IAsyncEnumerable<QueryConfigurationsResponse> CreateAsyncEnumerable(params QueryConfigurationsResponse[] responses)
    {
        foreach (QueryConfigurationsResponse response in responses)
        {
            yield return response;
        }

        await Task.CompletedTask;
    }
}