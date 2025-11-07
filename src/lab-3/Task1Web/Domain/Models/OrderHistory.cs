using Task1Web.Domain.Enums;
using Task1Web.Domain.Models.OrderHistoryPayloads;

namespace Task1Web.Domain.Models;

public class OrderHistory
{
    public long OrderHistoryItemId { get; set; }

    public long OrderId { get; set; }

    public DateTime OrderHistoryItemCreatedAt { get; set; }

    public OrderHistoryItemKind OrderHistoryItemKind { get; set; }

    public OrderHistoryPayload OrderHistoryItemPayload { get; set; }

    public OrderHistory(
        long orderHistoryItemId,
        long orderId,
        DateTime orderHistoryItemCreatedAt,
        OrderHistoryItemKind orderHistoryItemKind,
        OrderHistoryPayload orderHistoryItemPayload)
    {
        OrderHistoryItemId = orderHistoryItemId;
        OrderId = orderId;
        OrderHistoryItemCreatedAt = orderHistoryItemCreatedAt;
        OrderHistoryItemKind = orderHistoryItemKind;
        OrderHistoryItemPayload = orderHistoryItemPayload;
    }
}