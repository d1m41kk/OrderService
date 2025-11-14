using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Models.Orders.OrderHistoryPayloads;

public class StateChanged : OrderHistoryPayload
{
    public OrderState OrderStateOld { get; set; }

    public OrderState OrderStateNew { get; set; }
}