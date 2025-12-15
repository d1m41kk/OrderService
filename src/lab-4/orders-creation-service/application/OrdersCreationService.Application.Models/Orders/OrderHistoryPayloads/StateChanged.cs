using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads;

public class StateChanged : OrderHistoryPayload
{
    public OrderState OrderStateOld { get; set; }

    public OrderState OrderStateNew { get; set; }
}