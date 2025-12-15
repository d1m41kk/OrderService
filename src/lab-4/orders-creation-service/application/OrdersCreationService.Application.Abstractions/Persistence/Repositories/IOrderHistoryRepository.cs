using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Models.Orders;

namespace OrdersCreationService.Application.Abstractions.Persistence.Repositories;

public interface IOrderHistoryRepository
{
    Task<QueryOrderHistoryResponse> Find(
        QueryOrderHistorySearchRequest request,
        CancellationToken cancellationToken);

    Task<OrderHistory> Create(
        QueryOrderHistoryRequest request,
        CancellationToken cancellationToken);
}