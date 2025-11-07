using Npgsql;
using Task1Web.Domain.Enums;
using Task1Web.Domain.Interfaces;
using Task1Web.Domain.Models;

namespace Task1Web.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<QueryOrdersResponse> FindOrders(
        long? orderId,
        OrderState? state,
        string orderCreatedBy,
        int pageSize,
        string? pageToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        long cursor = 0;

        if (pageToken != null)
        {
            cursor = long.Parse(pageToken);
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

        command.Parameters.AddWithValue("@orderCreatedBy", $"%{orderCreatedBy}%");
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", pageSize);
        command.Parameters.AddWithValue("@state", state ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@orderId", orderId ?? (object)DBNull.Value);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        var orders = new List<Order>();
        long nextCursor = 0;
        while (await reader.ReadAsync())
        {
            nextCursor = reader.GetInt64(0);
            orders.Add(new Order(
                nextCursor,
                await reader.GetFieldValueAsync<OrderState>(1),
                reader.GetDateTime(2),
                reader.GetString(3)));
        }

        string? nextPageToken = null;

        if (orders.Count == pageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryOrdersResponse(orders, nextPageToken);
    }

    public async Task<long> CreateOrder(OrderState state, DateTime orderCreatedAt, string orderCreatedBy)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string query = """
                       INSERT INTO orders (order_state, order_created_at, order_created_by)
                       VALUES (@order_state, @order_created_at, @order_created_by)
                       RETURNING order_id
                       """;
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@order_state", state);
        command.Parameters.AddWithValue("@order_created_at", orderCreatedAt);
        command.Parameters.AddWithValue("@order_created_by", orderCreatedBy);
        object? id = await command.ExecuteScalarAsync();
        return Convert.ToInt64(id);
    }

    public async Task UpdateOrderState(long orderId, OrderState newState)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string query = """
                       UPDATE orders 
                       SET order_state = @newState
                       WHERE order_id = @orderId
                       """;

        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@newState", newState);
        command.Parameters.AddWithValue("@orderId", orderId);

        await command.ExecuteNonQueryAsync();
    }
}