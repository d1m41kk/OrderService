using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryOrderSearchRequest(
    int PageSize = 1,
    string OrderCreatedBy = "",
    long? OrderId = null,
    OrderState? State = null,
    string? PageToken = null);