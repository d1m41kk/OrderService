namespace Task1Web.Domain.Models.OrderHistoryPayloads;

public class ItemAdded : OrderCreated
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}