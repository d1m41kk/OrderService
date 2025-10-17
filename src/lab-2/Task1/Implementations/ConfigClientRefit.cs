using System.Runtime.CompilerServices;
using Task1.Interfaces;
using Task1.Models;

namespace Task1.Implementations;

public class ConfigClientRefit : IConfigsClient
{
    private readonly IApi _api;

    public ConfigClientRefit(IApi api)
    {
        _api = api;
    }

    public async ValueTask<QueryConfigurationsResponse?> GetConfigsFromPageAsync(int pageSize, string? pageToken, CancellationToken cancellationToken)
    {
        return await _api.GetConfigsFromPageAsync(pageSize, pageToken, cancellationToken);
    }

    public async IAsyncEnumerable<QueryConfigurationsResponse?> GetAllConfigsAsync(
        int pageSize,
        string? pageToken,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        QueryConfigurationsResponse? response = await GetConfigsFromPageAsync(pageSize, pageToken, cancellationToken);
        yield return response;

        while (MoveToNextPage(response))
        {
            string? currentToken = response?.PageToken;
            QueryConfigurationsResponse? next = await GetConfigsFromPageAsync(pageSize, currentToken, cancellationToken);

            if (next == null)
            {
                yield break;
            }

            yield return next;

            if (next.PageToken == currentToken)
            {
                yield break;
            }

            response = next;
        }
    }

    private static bool MoveToNextPage(QueryConfigurationsResponse? response)
    {
        return response?.PageToken != null;
    }
}