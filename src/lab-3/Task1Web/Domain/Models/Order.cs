using Task1Web.Domain.Enums;

namespace Task1Web.Domain.Models;

public class Order
{
    public long OrderId { get; set; }

    public OrderState OrderState { get; set; }

    public DateTime OrderCreatedAt { get; set; }

    public string OrderCreatedBy { get; set; }

    public Order(long orderId, OrderState orderState, DateTime orderCreatedAt, string orderCreatedBy)
    {
        OrderId = orderId;
        OrderState = orderState;
        OrderCreatedAt = orderCreatedAt;
        OrderCreatedBy = orderCreatedBy;
    }
}