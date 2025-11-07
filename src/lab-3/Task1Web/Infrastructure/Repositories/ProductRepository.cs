using Npgsql;
using Task1Web.Domain.Interfaces;
using Task1Web.Domain.Models;

namespace Task1Web.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<QueryProductsResponse> FindProducts(
        long productId,
        decimal? minPrice,
        decimal? maxPrice,
        string? subStringOfName,
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
                       SELECT product_id, product_name, product_price 
                                                FROM products 
                                                WHERE product_id = @productId
                                                   AND (@minPrice is null or product_price::numeric >= @minPrice) 
                                                   AND (@maxPrice is null or product_price::numeric <= @maxPrice) 
                                                   AND product_name LIKE @subStringOfName
                                                   AND product_id > @cursor
                                                ORDER BY product_id
                                                LIMIT @pageSize
                       """;

        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@productId", productId);
        command.Parameters.AddWithValue("@minPrice", minPrice ?? 0);
        command.Parameters.AddWithValue("@maxPrice", maxPrice ?? decimal.MaxValue);
        command.Parameters.AddWithValue("@subStringOfName", $"%{subStringOfName}%");
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", pageSize);
        NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        var products = new List<Product>();
        long nextCursor = 0;
        while (await reader.ReadAsync())
        {
            nextCursor = reader.GetInt64(0);
            products.Add(new Product(
                nextCursor,
                reader.GetString(1),
                reader.GetDecimal(2)));
        }

        string? nextPageToken = null;
        if (products.Count == pageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryProductsResponse(products, nextPageToken);
    }

    public async Task<long> CreateProduct(string name, decimal price)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync();

        string query = """
                       INSERT INTO products (product_name, product_price)
                       VALUES (@productName, @productPrice)
                       RETURNING product_id
                       """;

        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@productName", name);
        command.Parameters.AddWithValue("@productPrice", price);

        object? id = await command.ExecuteScalarAsync();
        return Convert.ToInt64(id);
    }
}