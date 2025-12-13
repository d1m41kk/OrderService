using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Application.Models.Orders;

public record Order(long OrderId, OrderState OrderState, DateTime OrderCreatedAt, string OrderCreatedBy);
