namespace Task1Web.Domain.Models;

public class OrderItem
{
    public long OrderItemId { get; set; }

    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public int OrderItemQuantity { get; set; }

    public bool OrderItemDeleted { get; set; }

    public OrderItem(long orderItemId, long orderId, long productId, int orderItemQuantity, bool orderItemDeleted)
    {
        OrderItemId = orderItemId;
        OrderId = orderId;
        ProductId = productId;
        OrderItemQuantity = orderItemQuantity;
        OrderItemDeleted = orderItemDeleted;
    }
}