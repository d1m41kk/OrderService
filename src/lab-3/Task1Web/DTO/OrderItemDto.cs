namespace Task1Web.DTO;

public record OrderItemDto(long OrderItemId, long OrderId, long ProductId, int OrderItemQuantity, bool OrderItemDeleted);