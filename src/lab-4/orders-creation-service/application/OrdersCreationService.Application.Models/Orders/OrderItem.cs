namespace OrdersCreationService.Application.Models.Orders;

public record OrderItem(long OrderItemId, long OrderId, long ProductId, int OrderItemQuantity, bool OrderItemDeleted);