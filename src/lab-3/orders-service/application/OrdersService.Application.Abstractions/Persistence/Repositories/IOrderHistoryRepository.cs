using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Models.Orders;

namespace OrdersService.Application.Abstractions.Persistence.Repositories;

public interface IOrderHistoryRepository
{
    Task<QueryOrderHistoryResponse> Find(
        QueryOrderHistorySearchRequest request,
        CancellationToken cancellationToken);

    Task<OrderHistory> Create(
        QueryOrderHistoryRequest request,
        CancellationToken cancellationToken);
}