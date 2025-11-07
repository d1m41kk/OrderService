using GetHistoryResponse = HttpGateway.Protos.GetHistoryResponse;

namespace HttpGateway.Interfaces;

public interface IGrpcOrderClient
{
    Task<long> CreateOrderAsync(string createdBy);

    Task AddProductToOrderAsync(long orderId, long productId, int quantity);

    Task RemoveProductFromOrderAsync(long orderId, long productId);

    Task ChangeOrderStatusAsync(long orderId, string status);

    Task<GetHistoryResponse> GetOrderHistoryAsync(long orderId, string? kind, int pageSize, string? pageToken);
}