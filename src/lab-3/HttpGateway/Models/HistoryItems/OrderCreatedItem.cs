namespace HttpGateway.Models.HistoryItems;

public record OrderCreatedItem(long Id, long OrderId, DateTime CreatedAt)
    : HistoryItem(Id, OrderId, CreatedAt, "order created");