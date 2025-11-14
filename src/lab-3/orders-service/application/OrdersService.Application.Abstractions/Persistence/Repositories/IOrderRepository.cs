using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Models.Orders;
using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Application.Abstractions.Persistence.Repositories;

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