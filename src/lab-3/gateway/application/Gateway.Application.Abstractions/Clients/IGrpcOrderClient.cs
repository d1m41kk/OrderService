using Gateway.Application.Models.Responses;
using OrdersService.Application.Models.Orders.Enums;

namespace Gateway.Application.Abstractions.Clients;

public interface IGrpcOrderClient
{
    Task<long> CreateOrderAsync(string createdBy, CancellationToken cancellationToken);

    Task AddProductToOrderAsync(long orderId, long productId, int quantity, CancellationToken cancellationToken);

    Task RemoveProductFromOrderAsync(long orderId, long productId, CancellationToken cancellationToken);

    Task ChangeOrderStatusAsync(long orderId, OrderState status, CancellationToken cancellationToken);

    Task<GetOrderHistoryResponse> GetOrderHistoryAsync(
        long orderId,
        OrderHistoryItemKind? kind,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken);
}