using System.Runtime.CompilerServices;
using Task1.Interfaces;
using Task1.Models;

namespace Task1.Implementations;

public class RefitClientConfigurationService : IConfigurationServiceClient
{
    private readonly IRefitClientConfigurationServiceApi _refitClientConfigurationServiceApi;

    public RefitClientConfigurationService(IRefitClientConfigurationServiceApi refitClientConfigurationServiceApi)
    {
        _refitClientConfigurationServiceApi = refitClientConfigurationServiceApi;
    }

    public async IAsyncEnumerable<QueryConfigurationsResponse> GetAllConfigsAsync(
        int pageSize,
        string? pageToken,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        QueryConfigurationsResponse response = await GetConfigsFromPageAsync(pageSize, pageToken, cancellationToken);
        do
        {
            string? currentToken = response.PageToken;
            QueryConfigurationsResponse next =
                await GetConfigsFromPageAsync(pageSize, currentToken, cancellationToken);

            yield return next;

            if (next.PageToken == currentToken)
            {
                yield break;
            }

            response = next;
        }
        while (MoveToNextPage(response));
    }

    private static bool MoveToNextPage(QueryConfigurationsResponse? response)
    {
        return response?.PageToken != null;
    }

    private async ValueTask<QueryConfigurationsResponse> GetConfigsFromPageAsync(
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken)
    {
        return await _refitClientConfigurationServiceApi.GetConfigsFromPageAsync(
            pageSize,
            pageToken,
            cancellationToken);
    }
}