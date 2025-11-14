namespace OrdersService.Application.Models.Orders.OrderHistoryPayloads;

public class ItemRemoved : OrderHistoryPayload
{
    public long ProductId { get; set; }
}