using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Application.Models.Orders.OrderHistoryPayloads;

namespace OrdersService.Application.Models.Orders;

public record OrderHistory(
    long OrderHistoryItemId,
    long OrderId,
    DateTime OrderHistoryItemCreatedAt,
    OrderHistoryItemKind OrderHistoryItemKind,
    OrderHistoryPayload OrderHistoryItemPayload);