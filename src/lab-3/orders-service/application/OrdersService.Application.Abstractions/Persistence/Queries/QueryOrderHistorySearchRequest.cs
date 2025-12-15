using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrderHistorySearchRequest(
    int PageSize = 1,
    string? PageToken = null,
    long? OrderId = null,
    OrderHistoryItemKind? Kind = null);