using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Abstractions.Persistence.Queries;

public record QueryOrderSearchRequest(
    int PageSize = 1,
    string OrderCreatedBy = "",
    long? OrderId = null,
    OrderState? State = null,
    string? PageToken = null);