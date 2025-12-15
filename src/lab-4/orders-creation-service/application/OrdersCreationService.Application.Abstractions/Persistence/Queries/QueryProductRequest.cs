namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryProductRequest(string Name, decimal Price);