using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Application.Models.Orders.OrderHistoryPayloads;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrderHistoryRequest(
    long OrderId,
    OrderHistoryPayload Payload,
    DateTime CreatedAt,
    OrderHistoryItemKind Kind);