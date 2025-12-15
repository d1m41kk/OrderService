using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryProductsResponse(IEnumerable<Product>? Products, string? PageToken);