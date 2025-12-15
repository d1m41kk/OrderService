using Grpc.Core;

namespace OrdersService.Presentation.Grpc.Services;

public class GrpcProductService : ProductService.ProductServiceBase
{
    private readonly ILogger<GrpcProductService> _logger;
    private readonly Application.Orders.ProductService _productService;

    public GrpcProductService(
        ILogger<GrpcProductService> logger,
        Application.Orders.ProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override async Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Create product with name: {Name} and with price: {Price}", request.Name, request.Price);
        Application.Models.Orders.Product product =
            await _productService.CreateProduct(request.Name, (decimal)request.Price, context.CancellationToken);
        return new CreateProductResponse
        {
            ProductId = product.ProductId,
        };
    }
}