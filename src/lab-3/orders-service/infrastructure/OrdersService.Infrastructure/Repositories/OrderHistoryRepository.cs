using Npgsql;
using NpgsqlTypes;
using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Abstractions.Persistence.Repositories;
using OrdersService.Application.Models.Orders;
using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Application.Models.Orders.OrderHistoryPayloads;
using OrdersService.Infrastructure.Dto;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrdersService.Infrastructure.Repositories;

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
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false,
        };
    }

    public async Task<QueryOrderHistoryResponse> Find(
        QueryOrderHistorySearchRequest request,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        long cursor = 0;

        if (!string.IsNullOrEmpty(request.PageToken))
        {
            cursor = long.Parse(request.PageToken);
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
        command.Parameters.AddWithValue("@orderId", request.OrderId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@orderHistoryItemKind", request.Kind ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", request.PageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        var history = new List<OrderHistory>();
        long nextCursor = 0;

        while (await reader.ReadAsync(cancellationToken))
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
        if (history.Count == request.PageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryOrderHistoryResponse(history, nextPageToken);
    }

    public async Task<OrderHistory> Create(
        QueryOrderHistoryRequest request,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        string json = JsonSerializer.Serialize(MapPayloadToDto(request.Payload), _jsonOptions);

        string query = """
                       INSERT INTO order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                       VALUES (@orderId, @orderHistoryItemCreatedAt, @orderHistoryItemKind, @orderHistoryItemPayload)
                       RETURNING order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                       """;
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("orderId", request.OrderId);
        command.Parameters.AddWithValue("@orderHistoryItemCreatedAt", request.CreatedAt);
        command.Parameters.AddWithValue("@orderHistoryItemKind", request.Kind);
        command.Parameters.AddWithValue("@orderHistoryItemPayload", NpgsqlDbType.Jsonb, json);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        JsonDocument jsonDoc = await reader.GetFieldValueAsync<JsonDocument>(4, cancellationToken);
        OrderHistoryPayload? payload = jsonDoc.Deserialize<OrderHistoryPayload>(_jsonOptions);

        if (payload == null)
        {
            throw new Exception("Order history item could not be created.");
        }

        return new OrderHistory(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetDateTime(2),
            reader.GetFieldValue<OrderHistoryItemKind>(3),
            payload);
    }

    private static OrderHistoryPayloadDto MapPayloadToDto(OrderHistoryPayload payload)
    {
        OrderHistoryPayloadDto orderHistoryDto = payload switch
        {
            OrderCreated created => new OrderCreatedDto()
            {
                CreatedBy = created.CreatedBy,
                OrderState = created.OrderState,
            },
            ItemAdded added => new ItemAddedDto()
            {
                ProductId = added.ProductId,
                Quantity = added.Quantity,
            },
            StateChanged stateChanged => new StateChangedDto()
            {
                OrderStateOld = stateChanged.OrderStateOld,
                OrderStateNew = stateChanged.OrderStateNew,
            },
            ItemRemoved removed => new ItemRemovedDto()
            {
                ProductId = removed.ProductId,
            },
            _ => throw new ArgumentException("Order history item could not be mapped."),
        };
        return orderHistoryDto;
    }
}