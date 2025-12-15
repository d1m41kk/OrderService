using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryProductsResponse(IEnumerable<Product>? Products, string? PageToken);