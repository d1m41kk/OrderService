namespace Gateway.Application.Models.HistoryItems;

public record ProductRemovedHistoryItem(long Id, long OrderId, DateTime CreatedAt, long ProductId)
    : HistoryItem(Id, OrderId, CreatedAt, "item_removed");