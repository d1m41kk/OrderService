using Task1.Models;

namespace Task1.Interfaces;

public interface IConfigurationServiceClient
{
    IAsyncEnumerable<QueryConfigurationsResponse> GetAllConfigsAsync(
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken);
}