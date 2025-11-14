using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrderRequest(
    OrderState State,
    DateTime OrderCreatedAt,
    string OrderCreatedBy);