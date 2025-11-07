using Task1Web.Domain.Models;

namespace Task1Web.Domain.Interfaces;

public interface IOrderItemRepository
{
    Task<QueryOrderItemsResponse> FindOrderItems(
        long? orderId,
        long? productId,
        bool? orderItemDeleted,
        int pageSize,
        string? pageToken);

    Task CreateOrderItem(
        long orderId,
        long productId,
        int orderItemQuantity);

    Task SoftDeleteOrderItem(
        long orderItemId);
}