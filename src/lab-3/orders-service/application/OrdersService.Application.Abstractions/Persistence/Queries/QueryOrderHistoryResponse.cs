using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrderHistoryResponse(IEnumerable<OrderHistory>? OrderHistory, string? PageToken);