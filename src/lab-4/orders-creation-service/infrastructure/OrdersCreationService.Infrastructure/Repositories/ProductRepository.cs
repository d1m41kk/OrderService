using Npgsql;
using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Abstractions.Persistence.Repositories;
using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<QueryProductsResponse> FindProducts(
        QueryProductSearchRequest request,
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
        command.Parameters.AddWithValue("@productId", request.ProductId);
        command.Parameters.AddWithValue("@minPrice", request.MinPrice ?? 0);
        command.Parameters.AddWithValue("@maxPrice", request.MaxPrice ?? decimal.MaxValue);
        command.Parameters.AddWithValue("@subStringOfName", $"%{request.SubStringOfName}%");
        command.Parameters.AddWithValue("@cursor", cursor);
        command.Parameters.AddWithValue("@pageSize", request.PageSize);
        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        var products = new List<Product>();
        long nextCursor = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            nextCursor = reader.GetInt64(0);
            products.Add(new Product(
                nextCursor,
                reader.GetString(1),
                reader.GetDecimal(2)));
        }

        string? nextPageToken = null;
        if (products.Count == request.PageSize)
        {
            nextPageToken = nextCursor.ToString();
        }

        return new QueryProductsResponse(products, nextPageToken);
    }

    public async Task<Product> CreateProduct(QueryProductRequest request, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = _dataSource.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        string query = """
                       INSERT INTO products (product_name, product_price)
                       VALUES (@productName, @productPrice)
                       RETURNING product_id, product_name, product_price
                       """;

        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@productName", request.Name);
        command.Parameters.AddWithValue("@productPrice", request.Price);

        NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new Product(reader.GetInt64(0), reader.GetString(1), reader.GetDecimal(2));
    }
}