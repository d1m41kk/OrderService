using System.Runtime.CompilerServices;
using System.Text.Json;
using Task1.Interfaces;
using Task1.Models;

namespace Task1.Implementations;

public class ConfigClientHttp : IConfigsClient
{
    private readonly JsonSerializerOptions? _options;
    private readonly HttpClient _httpClient;

    public ConfigClientHttp(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
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
            QueryConfigurationsResponse? next =
                await GetConfigsFromPageAsync(pageSize, currentToken, cancellationToken);

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

    public async ValueTask<QueryConfigurationsResponse?> GetConfigsFromPageAsync(
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken)
    {
        if (_httpClient.BaseAddress == null)
        {
            throw new NullReferenceException($"{nameof(_httpClient.BaseAddress)} is null");
        }

        Uri baseUri = _httpClient.BaseAddress;
        var uri = new Uri(
            baseUri,
            $"/configurations?pageSize={pageSize}" + $"&pageToken={pageToken}");

        HttpResponseMessage message = await _httpClient.GetAsync(uri, cancellationToken);
        string json = await message.Content.ReadAsStringAsync(cancellationToken);
        object? response = JsonSerializer.Deserialize<QueryConfigurationsResponse>(
            json,
            _options);
        return (QueryConfigurationsResponse?)response;
    }

    private static bool MoveToNextPage(QueryConfigurationsResponse? response)
    {
        return response?.PageToken != null;
    }
}