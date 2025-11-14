using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Abstractions.Persistence.Repositories;

public interface IProductRepository
{
    Task<QueryProductsResponse> FindProducts(
        QueryProductSearchRequest request,
        CancellationToken cancellationToken);

    Task<Product> CreateProduct(QueryProductRequest request, CancellationToken cancellationToken);
}