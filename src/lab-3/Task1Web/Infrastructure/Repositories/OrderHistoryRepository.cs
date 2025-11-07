using Npgsql;
using NpgsqlTypes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Task1Web.Domain.Enums;
using Task1Web.Domain.Interfaces;
using Task1Web.Domain.Models;
using Task1Web.Domain.Models.OrderHistoryPayloads;

namespace Task1Web.Infrastructure.Repositories;

public class OrderHistoryRepository : IOrderHistoryRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrderHistoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;

        _jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };
    }

    public async Task<QueryOrderHistoryResponse> Find(
        long? orderId,
        OrderHistoryItemKind? orderHistoryItemKind,
        int pageSize,
        string? pageToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        long cursor = 0;

        if (!string.IsNullOrEmpty(pageToken))
        {
            cursor = long.Parse(pageToken);
        }

        string query = """
                       SELECT *
                       FROM order_history
                       WHERE (@orderId is null or order_id = @orderId)
                       AND (@orderHistoryItemKind::order_history_item_kind is null or order_history_item_kind = @orderHistoryItemKind)
                       AND order_history_item_id > @cursor
                       ORDER BY order_history_item_id
                       LIMIT @pageSize
                       """;
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@orderId", orderId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@orderHistoryItemKind", orderHistoryItemKind ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", pageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        var history = new List<OrderHistory>();
        long nextCursor = 0;

        while (await reader.ReadAsync())
        {
            nextCursor = reader.GetInt64(0);

            JsonDocument jsonDocument = reader.GetFieldValue<JsonDocument>(4);
            OrderHistoryPayload? payload = jsonDocument.Deserialize<OrderHistoryPayload>(_jsonOptions);

            if (payload != null)
            {
                history.Add(new OrderHistory(
                    nextCursor,
                    reader.GetInt64(1),
                    reader.GetDateTime(2),
                    reader.GetFieldValue<OrderHistoryItemKind>(3),
                    payload));
            }
        }

        string? nextPageToken = null;
        if (history.Count == pageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryOrderHistoryResponse(history, nextPageToken);
    }

    public async Task CreateHistoryItem(
        long orderId,
        DateTime orderHistoryItemCreatedAt,
        OrderHistoryItemKind orderHistoryItemKind,
        OrderHistoryPayload orderHistoryItemPayload)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string json = JsonSerializer.Serialize(orderHistoryItemPayload, _jsonOptions);

        string query = """
                       INSERT INTO order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                       VALUES (@orderId, @orderHistoryItemCreatedAt, @orderHistoryItemKind, @orderHistoryItemPayload)
                       """;
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("orderId", orderId);
        command.Parameters.AddWithValue("@orderHistoryItemCreatedAt", orderHistoryItemCreatedAt);
        command.Parameters.AddWithValue("@orderHistoryItemKind", orderHistoryItemKind);
        command.Parameters.AddWithValue("@orderHistoryItemPayload", NpgsqlDbType.Jsonb, json);

        await command.ExecuteNonQueryAsync();
    }
}