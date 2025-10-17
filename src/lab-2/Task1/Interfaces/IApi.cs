using Refit;
using Task1.Models;

namespace Task1.Interfaces;

public interface IApi
{
    [Get("/configurations?pageSize={pageSize}&pageToken={pageToken}")]
    Task<QueryConfigurationsResponse?> GetConfigsFromPageAsync(
        [AliasAs("pageSize")] int pageSize,
        [AliasAs("pageToken")] string? pageToken,
        CancellationToken? cancellationToken = null);
}