using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Task1Web.Domain.Enums;

namespace Task1Web.Presentation.Grpc.Services;

public class GrpcOrderService : OrderService.OrderServiceBase
{
    private readonly ILogger<GrpcOrderService> _logger;
    private readonly Domain.Services.OrderService _orderService;

    public GrpcOrderService(ILogger<GrpcOrderService> logger, Domain.Services.OrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public override async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        _logger.LogInformation("CreateOrder called, user: {User}", request.CreatedBy);
        long orderId = await _orderService.CreateOrder(request.CreatedBy);
        return new CreateOrderResponse { OrderId = orderId };
    }

    public override async Task<AddProductResponse> AddProductToOrder(
        AddProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Adding product {ProductId} to order {OrderId}", request.ProductId, request.OrderId);
        await _orderService.AddProductToOrder(request.OrderId, request.ProductId, request.Quantity);
        return new AddProductResponse();
    }

    public override async Task<RemoveProductResponse> RemoveProductFromOrder(
        RemoveProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Removing product {ProductId} from order {OrderId}", request.ProductId, request.OrderId);
        await _orderService.DeleteProductInOrder(request.OrderId, request.ProductId);
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
            OrderStatus.Processing => _orderService.ChangeOrderStatusToProcessing(request.OrderId),
            OrderStatus.Completed => _orderService.ChangeOrderStatusToCompleted(request.OrderId),
            OrderStatus.Cancelled => _orderService.ChangeOrderStatusToCanceled(request.OrderId),
            OrderStatus.Created => throw new Exception("You cant change order status on created"),
            _ => throw new ArgumentOutOfRangeException($"Invalid status {request.Status}"),
        });
        return new ChangeStatusResponse();
    }

    public override async Task<GetHistoryResponse> GetOrderHistory(GetHistoryRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting history of order {OrderId}", request.OrderId);
        Domain.Models.QueryOrderHistoryResponse result =
            await _orderService.GetOrderHistory(request.OrderId, null, request.PageSize, request.PageToken);

        var response = new GetHistoryResponse();
        if (result.OrderHistory != null)
        {
            foreach (Domain.Models.OrderHistory orderHistory in result.OrderHistory)
            {
                response.HistoryItems.Add(ConvertToHistoryItem(orderHistory));
            }
        }

        response.NextPageToken = result.PageToken ?? string.Empty;
        return response;
    }

    private static HistoryItem ConvertToHistoryItem(Domain.Models.OrderHistory orderHistory)
    {
        var historyItem = new HistoryItem
        {
            Id = orderHistory.OrderHistoryItemId,
            OrderId = orderHistory.OrderId,
            CreatedAt = Timestamp.FromDateTime(orderHistory.OrderHistoryItemCreatedAt.ToUniversalTime()),
            Kind = ConvertToHistoryItemKind(orderHistory.OrderHistoryItemKind),
        };
        if (orderHistory.OrderHistoryItemPayload is Domain.Models.OrderHistoryPayloads.OrderCreated orderCreated)
        {
            historyItem.OrderCreated = new OrderCreatedPayload
            {
                CreatedBy = orderCreated.CreatedBy,
                CreatedAt = Timestamp.FromDateTime(orderCreated.CreatedAt.ToUniversalTime()),
            };
        }
        else if (orderHistory.OrderHistoryItemPayload is Domain.Models.OrderHistoryPayloads.ItemAdded itemAdded)
        {
            historyItem.ItemAdded = new ItemAddedPayload
            {
                ProductId = itemAdded.ProductId,
                Quantity = itemAdded.Quantity,
                CreatedAt = Timestamp.FromDateTime(itemAdded.CreatedAt.ToUniversalTime()),
            };
        }
        else if (orderHistory.OrderHistoryItemPayload is Domain.Models.OrderHistoryPayloads.ItemRemoved itemRemoved)
        {
            historyItem.ItemRemoved = new ItemRemovedPayload
            {
                ProductId = itemRemoved.ProductId,
                CreatedAt = Timestamp.FromDateTime(itemRemoved.CreatedAt.ToUniversalTime()),
            };
        }
        else if (orderHistory.OrderHistoryItemPayload is Domain.Models.OrderHistoryPayloads.StateChanged stateChanged)
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