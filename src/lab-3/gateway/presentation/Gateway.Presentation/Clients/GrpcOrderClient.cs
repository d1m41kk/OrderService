using Gateway.Application.Abstractions.Clients;
using Gateway.Application.Models.Responses;
using Gateway.Presentation.Extensions;
using Gateway.Presentation.Protos;
using OrdersService.Application.Models.Orders.Enums;
using CreateOrderResponse = Gateway.Presentation.Protos.CreateOrderResponse;

namespace Gateway.Presentation.Clients;

public class GrpcOrderClient : IGrpcOrderClient
{
    private readonly OrderService.OrderServiceClient _client;

    public GrpcOrderClient(OrderService.OrderServiceClient client)
    {
        _client = client;
    }

    public async Task<long> CreateOrderAsync(string createdBy, CancellationToken cancellationToken)
    {
        var request = new CreateOrderRequest
        {
            CreatedBy = createdBy,
        };
        CreateOrderResponse response = await _client.CreateOrderAsync(request, cancellationToken: cancellationToken);
        return response.OrderId;
    }

    public async Task AddProductToOrderAsync(
        long orderId,
        long productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var request = new AddProductRequest
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
        };
        await _client.AddProductToOrderAsync(request, cancellationToken: cancellationToken);
    }

    public async Task RemoveProductFromOrderAsync(long orderId, long productId, CancellationToken cancellationToken)
    {
        var request = new RemoveProductRequest
        {
            OrderId = orderId,
            ProductId = productId,
        };
        await _client.RemoveProductFromOrderAsync(request, cancellationToken: cancellationToken);
    }

    public async Task ChangeOrderStatusAsync(long orderId, OrderState status, CancellationToken cancellationToken)
    {
        var request = new ChangeStatusRequest
        {
            OrderId = orderId,
            Status = GrpcClientExtension.MapOrderStateToGrpcOrderStatus(status),
        };
        await _client.ChangeOrderStatusAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<GetOrderHistoryResponse> GetOrderHistoryAsync(
        long orderId,
        OrderHistoryItemKind? kind,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken)
    {
        var request = new GetHistoryRequest
        {
            OrderId = orderId,
            PageSize = pageSize,
            PageToken = pageToken ?? string.Empty,
            Kind = GrpcClientExtension.MapOrderHistoryItemKindToGrpcHistoryItemKind(kind),
        };

        GetHistoryResponse response = await _client.GetOrderHistoryAsync(request, cancellationToken: cancellationToken);
        return MapToOrderHistoryResponse(response);
    }

    private static GetOrderHistoryResponse MapToOrderHistoryResponse(GetHistoryResponse response)
    {
        var items = response.HistoryItems.Select(MapToHistoryItem).ToList();

        return new GetOrderHistoryResponse(items, response.NextPageToken);
    }

    private static Application.Models.HistoryItems.HistoryItem MapToHistoryItem(HistoryItem historyItem)
    {
        return new Application.Models.HistoryItems.HistoryItem(
            historyItem.Id,
            historyItem.OrderId,
            historyItem.CreatedAt.ToDateTime(),
            historyItem.Kind.ToString());
    }
}