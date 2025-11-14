using OrdersService.Application.Models.Orders.Enums;

namespace Gateway.Application.Models.Requests;

public record GetOrderHistoryRequest(long OrderId, OrderHistoryItemKind? Kind, int PageSize, string? PageToken);