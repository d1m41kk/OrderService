using Moq;
using Task1.Interfaces;
using Task1.Models;
using Task2.Implementations;
using Xunit;

namespace Lab2.Tests;

public class ProviderTests : IDisposable
{
    private readonly Mock<IConfigurationServiceClient> _mockClient;
    private readonly CustomConfigurationProvider _provider;
    private readonly PeriodicTimer _timer;
    private readonly CustomConfigurationService _service;

    public ProviderTests()
    {
        _mockClient = new Mock<IConfigurationServiceClient>();
        _provider = new CustomConfigurationProvider();
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        _service = new CustomConfigurationService(_provider, _mockClient.Object, _timer, 1);

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

        bool initialReloadResult = _provider.DoReload(response);
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
        var updatedItem = new ConfigurationItemDto("key1", "value2"); // Измененное значение
        var initialResponse = new QueryConfigurationsResponse([existingItem], null);
        var updatedResponse = new QueryConfigurationsResponse([updatedItem], null);

        _provider.DoReload(initialResponse);
        _provider.TryGet("key1", out string? initialValue);
        Assert.Equal("value1", initialValue);

        SetupClientResponse(updatedResponse);

        await _service.UpdateOnceAsync(CancellationToken.None);

        _provider.TryGet("key1", out string? updatedValue);
        Assert.Equal("value2", updatedValue);
        Assert.NotEqual(initialValue, updatedValue);
    }

    [Fact]
    public async Task ProviderHasKey_WhenReceiveEmptyCollection_ShouldBecomeEmptyAndReload()
    {
        // Arrange
        var existingItem = new ConfigurationItemDto("key1", "value1");
        var initialResponse = new QueryConfigurationsResponse([existingItem], null);
        var emptyResponse = new QueryConfigurationsResponse([], null);

        _provider.DoReload(initialResponse);
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

    private void SetupClientResponse(QueryConfigurationsResponse response)
    {
        IAsyncEnumerable<QueryConfigurationsResponse> asyncEnumerable = CreateAsyncEnumerable(response);
        _mockClient
            .Setup(c => c.GetAllConfigsAsync(It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(asyncEnumerable);
    }

    private async IAsyncEnumerable<QueryConfigurationsResponse> CreateAsyncEnumerable(QueryConfigurationsResponse response)
    {
        yield return response;
        await Task.CompletedTask;
    }
}