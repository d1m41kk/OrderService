using Task1Web.Domain.Interfaces;

namespace Task1Web.Domain.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<long> CreateProduct(string name, decimal price)
    {
        long id = await _productRepository.CreateProduct(name, price);
        return id;
    }
}