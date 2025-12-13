using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryOrderItemsResponse(IEnumerable<OrderItem>? OrderItems, string? PageToken);