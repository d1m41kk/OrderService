namespace Task1Web.Domain.Models;

public record QueryOrdersResponse(IEnumerable<Order>? Orders, string? PageToken);