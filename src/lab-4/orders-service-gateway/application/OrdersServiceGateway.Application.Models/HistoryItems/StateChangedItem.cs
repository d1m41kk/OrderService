namespace OrdersServiceGateway.Application.Models.HistoryItems;

public record StateChangedItem(long Id, long OrderId, DateTime CreatedAt, string OldState, string NewState)
    : HistoryItem(Id, OrderId, CreatedAt, "state_changed");