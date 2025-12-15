using Refit;
using Task1.Models;

namespace Task1.Interfaces;

public interface IRefitClientConfigurationServiceApi
{
    [Get("/configurations?pageSize={pageSize}&pageToken={pageToken}")]
    Task<QueryConfigurationsResponse> GetConfigsFromPageAsync(
        [AliasAs("pageSize")] int pageSize,
        [AliasAs("pageToken")] string? pageToken,
        CancellationToken? cancellationToken = null);
}