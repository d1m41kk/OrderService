using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Abstractions.Persistence.Repositories;
using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Orders;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Product> CreateProduct(string name, decimal price, CancellationToken token)
    {
        Product product = await _productRepository.CreateProduct(new QueryProductRequest(name, price), token);
        return product;
    }
}