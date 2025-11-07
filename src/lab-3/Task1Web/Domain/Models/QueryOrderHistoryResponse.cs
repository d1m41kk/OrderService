namespace Task1Web.Domain.Models;

public record QueryOrderHistoryResponse(IEnumerable<OrderHistory>? OrderHistory, string? PageToken);