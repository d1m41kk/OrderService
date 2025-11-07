using Grpc.Core;

namespace Task1Web.Presentation.Grpc.Services;

public class GrpcProductService : ProductService.ProductServiceBase
{
    private readonly ILogger<GrpcProductService> _logger;
    private readonly Domain.Services.ProductService _productService;

    public GrpcProductService(ILogger<GrpcProductService> logger, Domain.Services.ProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public override async Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Create product with name: {Name} and with price: {Price}", request.Name, request.Price);
        long productId = await _productService.CreateProduct(request.Name, (decimal)request.Price);
        return new CreateProductResponse
        {
            ProductId = productId,
        };
    }
}