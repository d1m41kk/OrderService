using Task1.Models;

namespace Task1.Interfaces;

public interface IConfigsClient
{
    ValueTask<QueryConfigurationsResponse?> GetConfigsFromPageAsync(int pageSize, string? pageToken, CancellationToken cancellationToken);

    IAsyncEnumerable<QueryConfigurationsResponse?> GetAllConfigsAsync(
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken);
}