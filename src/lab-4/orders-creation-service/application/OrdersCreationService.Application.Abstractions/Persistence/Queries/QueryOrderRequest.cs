using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryOrderRequest(
    OrderState State,
    DateTime OrderCreatedAt,
    string OrderCreatedBy);