namespace OrdersCreationService.Application.Abstractions.Persistence.Queries;

public record QueryProductSearchRequest(
    long ProductId,
    int PageSize = 1,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SubStringOfName = null,
    string? PageToken = null);