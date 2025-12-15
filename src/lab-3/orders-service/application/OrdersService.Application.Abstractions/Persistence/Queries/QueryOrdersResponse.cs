using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrdersResponse(IEnumerable<Order>? Orders, string? PageToken);