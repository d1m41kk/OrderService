using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Models.Orders;
using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Application.Models.Orders.OrderHistoryPayloads;

namespace OrdersService.Presentation.Grpc.Services;

public class GrpcOrderService : OrderService.OrderServiceBase
{
    private readonly ILogger<GrpcOrderService> _logger;
    private readonly Application.Orders.OrderService _orderService;

    public GrpcOrderService(ILogger<GrpcOrderService> logger, Application.Orders.OrderService orderService)
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

    public override async Task<AddProductResponse> AddProductToOrder(
        AddProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Adding product {ProductId} to order {OrderId}", request.ProductId, request.OrderId);
        await _orderService.AddProductToOrder(request.OrderId, request.ProductId, request.Quantity, context.CancellationToken);
        return new AddProductResponse();
    }

    public override async Task<RemoveProductResponse> RemoveProductFromOrder(
        RemoveProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Removing product {ProductId} from order {OrderId}", request.ProductId, request.OrderId);
        await _orderService.DeleteProductInOrder(request.OrderId, request.ProductId, context.CancellationToken);
        return new RemoveProductResponse();
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
            OrderStatus.Processing => _orderService.ChangeOrderStatusToProcessing(request.OrderId, context.CancellationToken),
            OrderStatus.Completed => _orderService.ChangeOrderStatusToCompleted(request.OrderId, context.CancellationToken),
            OrderStatus.Cancelled => _orderService.ChangeOrderStatusToCanceled(request.OrderId, context.CancellationToken),
            OrderStatus.Created => throw new Exception("You cant change order status on created"),
            OrderStatus.Unspecified => throw new Exception("You cant change order status unspecified"),
            _ => throw new ArgumentOutOfRangeException($"Invalid status {request.Status}"),
        });
        return new ChangeStatusResponse();
    }

    public override async Task<GetHistoryResponse> GetOrderHistory(GetHistoryRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting history of order {OrderId}", request.OrderId);
        QueryOrderHistoryResponse result =
            await _orderService.GetOrderHistory(request.OrderId, ConvertToOrderHistoryItemKind(request.Kind), request.PageSize, request.PageToken, context.CancellationToken);

        var response = new GetHistoryResponse();
        if (result.OrderHistory != null)
        {
            foreach (OrderHistory orderHistory in result.OrderHistory)
            {
                response.HistoryItems.Add(ConvertToHistoryItem(orderHistory));
            }
        }

        response.NextPageToken = result.PageToken ?? string.Empty;
        return response;
    }

    private static OrderHistoryItemKind? ConvertToOrderHistoryItemKind(HistoryItemKind kind)
    {
        OrderHistoryItemKind? orderHistoryItemKind = kind switch
        {
            HistoryItemKind.Unspecified => null,
            HistoryItemKind.CreatedItem => OrderHistoryItemKind.Created,
            HistoryItemKind.ItemAdded => OrderHistoryItemKind.ItemAdded,
            HistoryItemKind.ItemRemoved => OrderHistoryItemKind.ItemRemoved,
            HistoryItemKind.StateChanged => OrderHistoryItemKind.StateChanged,
            _ => throw new ArgumentOutOfRangeException($"Invalid history item kind {kind}"),
        };
        return orderHistoryItemKind;
    }

    private static HistoryItem ConvertToHistoryItem(OrderHistory orderHistory)
    {
        var historyItem = new HistoryItem
        {
            Id = orderHistory.OrderHistoryItemId,
            OrderId = orderHistory.OrderId,
            CreatedAt = Timestamp.FromDateTime(orderHistory.OrderHistoryItemCreatedAt.ToUniversalTime()),
            Kind = ConvertToHistoryItemKind(orderHistory.OrderHistoryItemKind),
        };
        if (orderHistory.OrderHistoryItemPayload is OrderCreated orderCreated)
        {
            historyItem.OrderCreated = new OrderCreatedPayload
            {
                CreatedBy = orderCreated.CreatedBy,
                CreatedAt = Timestamp.FromDateTime(orderCreated.CreatedAt.ToUniversalTime()),
            };
        }
        else if (orderHistory.OrderHistoryItemPayload is ItemAdded itemAdded)
        {
            historyItem.ItemAdded = new ItemAddedPayload
            {
                ProductId = itemAdded.ProductId,
                Quantity = itemAdded.Quantity,
                CreatedAt = Timestamp.FromDateTime(itemAdded.CreatedAt.ToUniversalTime()),
            };
        }
        else if (orderHistory.OrderHistoryItemPayload is ItemRemoved itemRemoved)
        {
            historyItem.ItemRemoved = new ItemRemovedPayload
            {
                ProductId = itemRemoved.ProductId,
                CreatedAt = Timestamp.FromDateTime(itemRemoved.CreatedAt.ToUniversalTime()),
            };
        }
        else if (orderHistory.OrderHistoryItemPayload is StateChanged stateChanged)
        {
            historyItem.StateChanged = new StateChangedPayload
            {
                OldState = ConvertToGrpcStatus(stateChanged.OrderStateOld),
                NewState = ConvertToGrpcStatus(stateChanged.OrderStateNew),
            };
        }

        return historyItem;
    }

    private static HistoryItemKind ConvertToHistoryItemKind(OrderHistoryItemKind orderHistoryItemKind)
    {
        return orderHistoryItemKind switch
        {
            OrderHistoryItemKind.Created => HistoryItemKind.CreatedItem,
            OrderHistoryItemKind.ItemAdded => HistoryItemKind.ItemAdded,
            OrderHistoryItemKind.ItemRemoved => HistoryItemKind.ItemRemoved,
            OrderHistoryItemKind.StateChanged => HistoryItemKind.StateChanged,
            _ => HistoryItemKind.CreatedItem,
        };
    }

    private static OrderStatus ConvertToGrpcStatus(OrderState state)
    {
        return state switch
        {
            OrderState.Created => OrderStatus.Created,
            OrderState.Processing => OrderStatus.Processing,
            OrderState.Completed => OrderStatus.Completed,
            OrderState.Cancelled => OrderStatus.Cancelled,
            _ => OrderStatus.Created,
        };
    }
}