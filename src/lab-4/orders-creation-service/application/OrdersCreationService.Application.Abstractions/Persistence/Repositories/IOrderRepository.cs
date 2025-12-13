using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Models.Orders;
using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Application.Abstractions.Persistence.Repositories;

public interface IOrderRepository
{
    Task<QueryOrdersResponse> FindOrders(
        QueryOrderSearchRequest request,
        CancellationToken cancellationToken);

    Task<Order> CreateOrder(
        QueryOrderRequest request,
        CancellationToken cancellationToken);

    Task UpdateOrderState(long orderId, OrderState newState, CancellationToken cancellationToken);
}