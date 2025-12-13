using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryOrderHistoryRequest(
    long OrderId,
    OrderHistoryPayload Payload,
    DateTime CreatedAt,
    OrderHistoryItemKind Kind);