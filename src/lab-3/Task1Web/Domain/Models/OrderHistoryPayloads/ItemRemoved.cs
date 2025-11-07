namespace Task1Web.Domain.Models.OrderHistoryPayloads;

public class ItemRemoved : OrderHistoryPayload
{
    public long ProductId { get; set; }
}