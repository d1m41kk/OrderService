namespace OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads;

public class ItemAdded : OrderHistoryPayload
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}