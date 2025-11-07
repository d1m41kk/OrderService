using Grpc.Net.Client;
using HttpGateway.Interfaces;
using HttpGateway.Protos;

namespace HttpGateway.Clients;

public class GrpcOrderClient : IGrpcOrderClient
{
    private readonly OrderService.OrderServiceClient _client;

    public GrpcOrderClient(GrpcChannel channel)
    {
        _client = new OrderService.OrderServiceClient(channel);
    }

    public async Task<long> CreateOrderAsync(string createdBy)
    {
        var request = new CreateOrderRequest
        {
            CreatedBy = createdBy,
        };
        CreateOrderResponse response = await _client.CreateOrderAsync(request);
        return response.OrderId;
    }

    public async Task AddProductToOrderAsync(long orderId, long productId, int quantity)
    {
        var request = new AddProductRequest
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };
        await _client.AddProductToOrderAsync(request);
    }

    public async Task RemoveProductFromOrderAsync(long orderId, long productId)
    {
        var request = new RemoveProductRequest
        {
            OrderId = orderId,
            ProductId = productId,
        };
        await _client.RemoveProductFromOrderAsync(request);
    }

    public async Task ChangeOrderStatusAsync(long orderId, string status)
    {
        OrderStatus grpcStatus = status.ToUpperInvariant() switch
        {
            "PROCESSING" => OrderStatus.Processing,
            "COMPLETED" => OrderStatus.Completed,
            "CREATED" => OrderStatus.Created,
            "CANCELLED" => OrderStatus.Cancelled,
            _ => throw new ArgumentException("Unknown order status"),
        };
        var request = new ChangeStatusRequest
        {
            OrderId = orderId,
            Status = grpcStatus,
        };
        await _client.ChangeOrderStatusAsync(request);
    }

    public async Task<GetHistoryResponse> GetOrderHistoryAsync(
        long orderId,
        string? kind,
        int pageSize,
        string? pageToken)
    {
        var request = new GetHistoryRequest
        {
            OrderId = orderId,
            PageSize = pageSize,
            PageToken = pageToken ?? string.Empty,
        };

        if (!string.IsNullOrEmpty(kind))
        {
            request.Kind = kind.ToUpperInvariant() switch
            {
                "CREATED_ITEM" => HistoryItemKind.CreatedItem,
                "ITEM_ADDED" => HistoryItemKind.ItemAdded,
                "ITEM_REMOVED" => HistoryItemKind.ItemRemoved,
                "STATE_CHANGED" => HistoryItemKind.StateChanged,
                _ => throw new ArgumentException("Unknown kind"),
            };
        }

        GetHistoryResponse response = await _client.GetOrderHistoryAsync(request);
        return response;
    }
}