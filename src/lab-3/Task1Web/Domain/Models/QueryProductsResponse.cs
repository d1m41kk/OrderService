namespace Task1Web.Domain.Models;

public record QueryProductsResponse(IEnumerable<Product>? Products, string? PageToken);