using Task1Web.Domain.Enums;
using Task1Web.Domain.Models;

namespace Task1Web.Domain.Interfaces;

public interface IOrderRepository
{
    Task<QueryOrdersResponse> FindOrders(
        long? orderId,
        OrderState? state,
        string orderCreatedBy,
        int pageSize,
        string? pageToken);

    Task<long> CreateOrder(OrderState state, DateTime orderCreatedAt, string orderCreatedBy);

    Task UpdateOrderState(long orderId, OrderState newState);
}