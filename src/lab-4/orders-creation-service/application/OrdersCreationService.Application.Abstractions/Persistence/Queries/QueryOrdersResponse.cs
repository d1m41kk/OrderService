using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryOrdersResponse(IEnumerable<Order>? Orders, string? PageToken);