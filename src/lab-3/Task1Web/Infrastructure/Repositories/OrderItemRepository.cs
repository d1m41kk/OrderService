using Npgsql;
using Task1Web.Domain.Interfaces;
using Task1Web.Domain.Models;

namespace Task1Web.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderItemRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<QueryOrderItemsResponse> FindOrderItems(
        long? orderId,
        long? productId,
        bool? orderItemDeleted,
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
                       select * 
                       FROM order_items 
                       WHERE (@orderId is null or order_id = @orderId)
                       AND (@productId is null or product_id = @productId)
                       AND (@orderItemDeleted is null or order_item_deleted = @orderItemDeleted)
                       AND order_item_id > @cursor
                       ORDER BY order_item_id
                       LIMIT @pageSize
                       """;
        var command = new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("@orderId", orderId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@productId", productId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@orderItemDeleted", orderItemDeleted ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", pageSize);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        var orderItems = new List<OrderItem>();
        long nextCursor = 0;
        while (await reader.ReadAsync())
        {
            nextCursor = reader.GetInt64(0);
            orderItems.Add(new OrderItem(
                nextCursor,
                reader.GetInt64(1),
                reader.GetInt64(2),
                reader.GetInt32(3),
                reader.GetBoolean(4)));
        }

        string? nextPageToken = null;

        if (orderItems.Count == pageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryOrderItemsResponse(orderItems, nextPageToken);
    }

    public async Task CreateOrderItem(
        long orderId,
        long productId,
        int orderItemQuantity)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string query = """
                       INSERT INTO order_items (order_id, product_id, order_item_quantity, order_item_deleted) 
                       VALUES (@orderId, @productId, @orderItemQuantity, false)
                       """;
        var command = new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("@orderId", orderId);
        command.Parameters.AddWithValue("@productId", productId);
        command.Parameters.AddWithValue("@orderItemQuantity", orderItemQuantity);

        await command.ExecuteNonQueryAsync();
    }

    public async Task SoftDeleteOrderItem(
        long orderItemId)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string query = """
                       UPDATE order_items
                       SET order_item_deleted = true
                       WHERE order_item_id = @orderItemId
                       """;
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@orderItemId", orderItemId);
        await command.ExecuteNonQueryAsync();
    }
}