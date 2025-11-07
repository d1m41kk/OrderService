namespace Task1Web.Domain.Models;

public record QueryOrderItemsResponse(IEnumerable<OrderItem>? OrderItems, string? PageToken);