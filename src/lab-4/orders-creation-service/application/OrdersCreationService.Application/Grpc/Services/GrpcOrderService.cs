using Grpc.Core;
using Microsoft.Extensions.Logging;
using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersCreationService.Application.Orders;
using OrdersCreationServiceGrpc.Contracts;

namespace OrdersCreationService.Application.Grpc.Services;

public class GrpcOrderService : OrderCreationService.OrderCreationServiceBase
{
    private readonly ILogger<GrpcOrderService> _logger;
    private readonly OrdersCreatingService _orderService;

    public GrpcOrderService(ILogger<GrpcOrderService> logger, OrdersCreatingService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        _logger.LogInformation("CreateOrder called, user: {User}", request.CreatedBy);
        long orderId = await _orderService.CreateOrder(request.CreatedBy, context.CancellationToken);
        return new CreateOrderResponse { OrderId = orderId };
    }

    public override async Task<ChangeStatusResponse> ChangeOrderStatus(
        ChangeStatusRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "Changing status of order {OrderId} with status {Status}",
            request.OrderId,
            request.Status);
        await (request.Status switch
        {
            OrderStatus.Processing => _orderService.ChangeOrderStatusToProcessing(
                request.OrderId,
                context.CancellationToken),
            OrderStatus.Completed => _orderService.ChangeOrderStatusToCompleted(
                request.OrderId,
                context.CancellationToken),
            OrderStatus.Cancelled => _orderService.ChangeOrderStatusToCanceled(
                request.OrderId,
                context.CancellationToken),
            OrderStatus.Created => throw new Exception("You cant change order status on created"),
            OrderStatus.Unspecified => throw new Exception("You cant change order status unspecified"),
            _ => throw new ArgumentOutOfRangeException($"Invalid status {request.Status}"),
        });
        return new ChangeStatusResponse();
    }

    public override async Task<CancelOrderResponse> CancelOrder(CancelOrderRequest request, ServerCallContext context)
    {
        if (await _orderService.GetOrderState(request.OrderId, context.CancellationToken) is OrderState.Created)
        {
            await _orderService.ChangeOrderStatusToCanceled(orderId: request.OrderId, context.CancellationToken);
            return new CancelOrderResponse()
            {
                Success = true,
            };
        }

        return new CancelOrderResponse()
        {
            Success = false,
        };
    }
}