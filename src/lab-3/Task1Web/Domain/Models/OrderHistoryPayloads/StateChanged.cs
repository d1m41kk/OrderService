using Task1Web.Domain.Enums;

namespace Task1Web.Domain.Models.OrderHistoryPayloads;

public class StateChanged : OrderHistoryPayload
{
    public OrderState OrderStateOld { get; set; }

    public OrderState OrderStateNew { get; set; }
}