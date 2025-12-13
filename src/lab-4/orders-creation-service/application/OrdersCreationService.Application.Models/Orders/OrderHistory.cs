using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads;

namespace OrdersCreationService.Application.Models.Orders;

public record OrderHistory(
    long OrderHistoryItemId,
    long OrderId,
    DateTime OrderHistoryItemCreatedAt,
    OrderHistoryItemKind OrderHistoryItemKind,
    OrderHistoryPayload OrderHistoryItemPayload);