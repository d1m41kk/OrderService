namespace OrdersServiceGateway.Application.Models.HistoryItems;

public record HistoryItem(long Id, long OrderId, DateTime CreatedAt, string Kind);