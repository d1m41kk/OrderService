using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrderItemsResponse(IEnumerable<OrderItem>? OrderItems, string? PageToken);