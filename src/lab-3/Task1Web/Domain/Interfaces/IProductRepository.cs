using Task1Web.Domain.Models;

namespace Task1Web.Domain.Interfaces;

public interface IProductRepository
{
    Task<QueryProductsResponse> FindProducts(
        long productId,
        decimal? minPrice,
        decimal? maxPrice,
        string? subStringOfName,
        int pageSize,
        string? pageToken);

    Task<long> CreateProduct(string name, decimal price);
}