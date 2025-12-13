using Gateway.Kafka.Contracts;

namespace OrdersServiceGateway.Presentation.Clients;

public class GrpcOrderProcessingClient
{
    private readonly OrderService.OrderServiceClient _client;

    public GrpcOrderProcessingClient(OrderService.OrderServiceClient client)
    {
        _client = client;
    }

    public async Task ApproveOrderAsync(
        long orderId,
        bool isApproved,
        string approvedBy,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ApproveOrderRequest
        {
            OrderId = orderId,
            IsApproved = isApproved,
            ApprovedBy = approvedBy,
            FailureReason = failureReason,
        };

        await _client.ApproveOrderAsync(request, cancellationToken: cancellationToken);
    }

    public async Task StartOrderPackingAsync(
        long orderId,
        string packingBy,
        CancellationToken cancellationToken = default)
    {
        var request = new StartOrderPackingRequest
        {
            OrderId = orderId,
            PackingBy = packingBy,
        };

        await _client.StartOrderPackingAsync(request, cancellationToken: cancellationToken);
    }

    public async Task FinishOrderPackingAsync(
        long orderId,
        bool isSuccessful,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var request = new FinishOrderPackingRequest
        {
            OrderId = orderId,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
        };

        await _client.FinishOrderPackingAsync(request, cancellationToken: cancellationToken);
    }

    public async Task StartOrderDeliveryAsync(
        long orderId,
        string deliveredBy,
        CancellationToken cancellationToken = default)
    {
        var request = new StartOrderDeliveryRequest
        {
            OrderId = orderId,
            DeliveredBy = deliveredBy,
        };

        await _client.StartOrderDeliveryAsync(request, cancellationToken: cancellationToken);
    }

    public async Task FinishOrderDeliveryAsync(
        long orderId,
        bool isSuccessful,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var request = new FinishOrderDeliveryRequest
        {
            OrderId = orderId,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
        };

        await _client.FinishOrderDeliveryAsync(request, cancellationToken: cancellationToken);
    }
}