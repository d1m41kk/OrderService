namespace Gateway.Application.Models.HistoryItems;

public record ProductAddedItem(long Id, long OrderId, DateTime CreatedAt, long ProductId)
    : HistoryItem(Id, OrderId, CreatedAt, "product_added");