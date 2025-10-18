using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Task1.Interfaces;
using Task1.Models;

namespace Task1.Implementations;

public class HttpClientConfigurationService : IConfigurationServiceClient
{
    private readonly JsonSerializerOptions? _options;
    private readonly HttpClient _httpClient;

    public HttpClientConfigurationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public async IAsyncEnumerable<QueryConfigurationsResponse> GetAllConfigsAsync(
        int pageSize,
        string? pageToken,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? currentToken = pageToken;

        do
        {
            QueryConfigurationsResponse next =
                await GetConfigsFromPageAsync(pageSize, currentToken, cancellationToken);

            yield return next;

            if (next.PageToken == currentToken)
            {
                yield break;
            }

            currentToken = next.PageToken;
        }
        while (MoveToNextPage(await GetConfigsFromPageAsync(pageSize, currentToken, cancellationToken)));
    }

    private static bool MoveToNextPage(QueryConfigurationsResponse response)
    {
        return response.PageToken != null;
    }

    private async ValueTask<QueryConfigurationsResponse> GetConfigsFromPageAsync(
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken)
    {
        Uri? baseUri = _httpClient.BaseAddress;
        var uri = new Uri(
            baseUri ?? throw new InvalidOperationException(),
            $"/configurations?pageSize={pageSize}" + $"&pageToken={pageToken}");

        HttpResponseMessage message = await _httpClient.GetAsync(uri, cancellationToken);
        QueryConfigurationsResponse? response = await message.Content.ReadFromJsonAsync<QueryConfigurationsResponse>(
            _options,
            cancellationToken);
        return response ?? throw new NullReferenceException($"{nameof(response)} is null");
    }
}