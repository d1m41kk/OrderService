using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryOrderHistoryResponse(IEnumerable<OrderHistory>? OrderHistory, string? PageToken);