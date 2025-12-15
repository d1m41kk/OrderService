using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Repositories;

public interface IProductRepository
{
    Task<QueryProductsResponse> FindProducts(
        QueryProductSearchRequest request,
        CancellationToken cancellationToken);

    Task<Product> CreateProduct(QueryProductRequest request, CancellationToken cancellationToken);
}