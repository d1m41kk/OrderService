using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Models.Orders.OrderHistoryPayloads;

public class OrderCreated : OrderHistoryPayload
{
    public string CreatedBy { get; set; } = string.Empty;

    public OrderState OrderState { get; set; }
}