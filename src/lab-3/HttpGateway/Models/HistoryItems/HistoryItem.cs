namespace HttpGateway.Models.HistoryItems;

public record HistoryItem(long Id, long OrderId, DateTime CreatedAt, string Kind);