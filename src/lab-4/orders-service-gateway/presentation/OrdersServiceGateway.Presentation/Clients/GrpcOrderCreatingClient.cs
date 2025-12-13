using Gateway.Kafka.Contracts;
using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersServiceGateway.Applicarion.Abstractions.Clients;
using OrdersServiceGateway.Presentation.Extensions;

namespace OrdersServiceGateway.Presentation.Clients;

public class GrpcOrderCreatingClient : IGrpcOrderClient
{
    private readonly OrderCreationService.OrderCreationServiceClient _client;

    public GrpcOrderCreatingClient(OrderCreationService.OrderCreationServiceClient client)
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

    public async Task ChangeOrderStatusAsync(long orderId, OrderState status, CancellationToken cancellationToken)
    {
        var request = new ChangeStatusRequest
        {
            OrderId = orderId,
            Status = GrpcClientExtension.MapOrderStateToGrpcOrderStatus(status),
        };
        await _client.ChangeOrderStatusAsync(request, cancellationToken: cancellationToken);
    }

    public async Task<bool> CancelOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var request = new CancelOrderRequest
        {
            OrderId = orderId,
        };
        CancelOrderResponse response = await _client.CancelOrderAsync(request, cancellationToken: cancellationToken);
        return response.Success;
    }
}