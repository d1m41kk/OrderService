using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Models.Orders;

public record Order(long OrderId, OrderState OrderState, DateTime OrderCreatedAt, string OrderCreatedBy);
