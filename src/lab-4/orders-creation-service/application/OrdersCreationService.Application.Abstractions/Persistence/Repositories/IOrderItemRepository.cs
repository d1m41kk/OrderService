using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Repositories;

public interface IOrderItemRepository
{
    Task<QueryOrderItemsResponse> FindOrderItems(
        long? orderId,
        long? productId,
        bool? orderItemDeleted,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken);

    Task<OrderItem> CreateOrderItem(
        long orderId,
        long productId,
        int orderItemQuantity,
        CancellationToken cancellationToken);

    Task SoftDeleteOrderItem(
        long orderItemId,
        CancellationToken cancellationToken);
}