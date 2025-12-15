using Microsoft.Extensions.Logging;
using Orders.Kafka.Contracts;
using OrdersCreationService.Application.Orders;

namespace OrdersCreationService.Application.KafkaHandlers;

public class OrderProcessingEventsHandler
{
    private readonly OrdersCreatingService _orderService;
    private readonly ILogger<OrderProcessingEventsHandler> _logger;

    public OrderProcessingEventsHandler(OrdersCreatingService orderService, ILogger<OrderProcessingEventsHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task HandleAsync(
        OrderProcessingKey key,
        OrderProcessingValue message,
        CancellationToken cancellationToken)
    {
        switch (message.EventCase)
        {
            case OrderProcessingValue.EventOneofCase.ApprovalReceived:
                _logger.LogInformation("Received approval message");
                if (!message.ApprovalReceived.IsApproved)
                {
                    await _orderService.ChangeOrderStatusToCanceled(message.ApprovalReceived.OrderId, cancellationToken);
                    _logger.LogInformation("Canceled approval message");
                }

                break;
            case OrderProcessingValue.EventOneofCase.PackingStarted:
                _logger.LogInformation("Packing started message");
                break;
            case OrderProcessingValue.EventOneofCase.PackingFinished:
                if (!message.PackingFinished.IsFinishedSuccessfully)
                {
                    await _orderService.ChangeOrderStatusToCanceled(message.PackingFinished.OrderId, cancellationToken);
                    _logger.LogInformation($"Packing failed message, the reason: {message.PackingFinished.FailureReason}");
                }
                else
                {
                    _logger.LogInformation("Packing finished message");
                }

                break;
            case OrderProcessingValue.EventOneofCase.DeliveryStarted:
                _logger.LogInformation("Delivery started message");
                break;
            case OrderProcessingValue.EventOneofCase.DeliveryFinished:
                if (!message.DeliveryFinished.IsFinishedSuccessfully)
                {
                    await _orderService.ChangeOrderStatusToCanceled(message.DeliveryFinished.OrderId, cancellationToken);
                    _logger.LogInformation($"Delivery failed message, the reason: {message.DeliveryFinished.FailureReason}");
                }
                else
                {
                    await _orderService.ChangeOrderStatusToCompleted(key.OrderId, cancellationToken);
                    _logger.LogInformation("Delivery finished message");
                }

                break;
            case OrderProcessingValue.EventOneofCase.None:
                break;
            default:
                _logger.LogError("Unhandled order processing event");
                break;
        }
    }
}