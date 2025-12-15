using Npgsql;
using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Abstractions.Persistence.Repositories;
using OrdersCreationService.Application.Models.Orders;
using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<QueryOrdersResponse> FindOrders(
        QueryOrderSearchRequest request,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        long cursor = 0;

        if (request.PageToken != null)
        {
            cursor = long.Parse(request.PageToken);
        }

        string query = """
                       SELECT * 
                       FROM orders 
                       WHERE (@orderId is null or order_id = @orderId)
                       AND (@state::order_state is null or order_state = @state) 
                       AND order_created_by LIKE @orderCreatedBy
                       AND order_id > @cursor
                       ORDER BY order_id
                       LIMIT @pageSize
                       """;
        var command = new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("@orderCreatedBy", $"%{request.OrderCreatedBy}%");
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", request.PageSize);
        command.Parameters.AddWithValue("@state", request.State ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@orderId", request.OrderId ?? (object)DBNull.Value);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var orders = new List<Order>();
        long nextCursor = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            nextCursor = reader.GetInt64(0);
            orders.Add(new Order(
                nextCursor,
                await reader.GetFieldValueAsync<OrderState>(1, cancellationToken),
                reader.GetDateTime(2),
                reader.GetString(3)));
        }

        string? nextPageToken = null;

        if (orders.Count == request.PageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryOrdersResponse(orders, nextPageToken);
    }

    public async Task<Order> CreateOrder(
        QueryOrderRequest request,
        CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        string query = """
                       INSERT INTO orders (order_state, order_created_at, order_created_by)
                       VALUES (@order_state, @order_created_at, @order_created_by)
                       RETURNING order_id, order_state, order_created_at, order_created_by
                       """;
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@order_state", request.State);
        command.Parameters.AddWithValue("@order_created_at", request.OrderCreatedAt);
        command.Parameters.AddWithValue("@order_created_by", request.OrderCreatedBy);
        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new Order(reader.GetInt64(0), reader.GetFieldValue<OrderState>(1), reader.GetDateTime(2), reader.GetString(3));
    }

    public async Task UpdateOrderState(long orderId, OrderState newState, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        string query = """
                       UPDATE orders 
                       SET order_state = @newState
                       WHERE order_id = @orderId
                       """;

        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@newState", newState);
        command.Parameters.AddWithValue("@orderId", orderId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}