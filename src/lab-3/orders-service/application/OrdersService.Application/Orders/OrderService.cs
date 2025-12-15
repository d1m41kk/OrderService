using OrdersService.Application.Abstractions.Persistence.Queries;
using OrdersService.Application.Abstractions.Persistence.Repositories;
using OrdersService.Application.Models.Orders;
using OrdersService.Application.Models.Orders.Enums;
using OrdersService.Application.Models.Orders.OrderHistoryPayloads;
using System.Transactions;

namespace OrdersService.Application.Orders;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderHistoryRepository _orderHistoryRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IProductRepository productRepository,
        IOrderHistoryRepository orderHistoryRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _orderHistoryRepository = orderHistoryRepository;
    }

    public async Task<long> CreateOrder(string orderCreatedBy, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        DateTime orderCreatedAt = DateTime.Now;

        Order order = await _orderRepository.CreateOrder(
            new QueryOrderRequest(OrderState.Created, orderCreatedAt, orderCreatedBy),
            cancellationToken);

        var payload = new OrderCreated
        {
            CreatedBy = orderCreatedBy,
            CreatedAt = orderCreatedAt,
        };

        await _orderHistoryRepository.Create(
            new QueryOrderHistoryRequest(
                order.OrderId,
                payload,
                DateTime.Now,
                OrderHistoryItemKind.Created),
            cancellationToken);
        scope.Complete();
        return order.OrderId;
    }

    public async Task AddProductToOrder(
        long orderId,
        long productId,
        int orderItemQuantity,
        CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        QueryProductsResponse productsResponse =
            await _productRepository.FindProducts(new QueryProductSearchRequest(orderId), cancellationToken);
        if (productsResponse.Products != null)
        {
            QueryOrdersResponse ordersResponse =
                await _orderRepository.FindOrders(
                    new QueryOrderSearchRequest(OrderId: orderId, State: OrderState.Created),
                    cancellationToken);
            if (ordersResponse.Orders != null)
            {
                await _orderItemRepository.CreateOrderItem(orderId, productId, orderItemQuantity, cancellationToken);

                var payload = new ItemAdded
                {
                    ProductId = productId,
                    Quantity = orderItemQuantity,
                    CreatedAt = DateTime.Now,
                };
                await _orderHistoryRepository.Create(
                    new QueryOrderHistoryRequest(
                        orderId,
                        payload,
                        DateTime.Now,
                        OrderHistoryItemKind.ItemAdded),
                    cancellationToken);
            }
        }

        scope.Complete();
    }

    public async Task DeleteProductInOrder(long orderId, long productId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        QueryProductsResponse productsResponse =
            await _productRepository.FindProducts(new QueryProductSearchRequest(orderId), cancellationToken);
        if (productsResponse.Products != null)
        {
            QueryOrdersResponse ordersResponse =
                await _orderRepository.FindOrders(
                    new QueryOrderSearchRequest(OrderId: orderId, State: OrderState.Created),
                    cancellationToken);
            if (ordersResponse.Orders != null)
            {
                QueryOrderItemsResponse itemsResponse =
                    await _orderItemRepository.FindOrderItems(orderId, productId, false, 1, null, cancellationToken);
                OrderItem? item = itemsResponse.OrderItems?.FirstOrDefault();
                if (item != null)
                {
                    await _orderItemRepository.SoftDeleteOrderItem(item.OrderItemId, cancellationToken);
                    var payload = new ItemRemoved
                    {
                        ProductId = productId,
                        CreatedAt = DateTime.Now,
                    };
                    await _orderHistoryRepository.Create(
                        new QueryOrderHistoryRequest(
                            orderId,
                            payload,
                            DateTime.Now,
                            OrderHistoryItemKind.ItemRemoved),
                        cancellationToken);
                }
            }
        }

        scope.Complete();
    }

    public async Task ChangeOrderStatusToProcessing(long orderId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        QueryOrdersResponse orderResponse =
            await _orderRepository.FindOrders(new QueryOrderSearchRequest(OrderId: orderId), cancellationToken);
        Order? order = orderResponse.Orders?.FirstOrDefault();
        if (order != null && order.OrderState != OrderState.Processing)
        {
            await _orderRepository.UpdateOrderState(orderId, OrderState.Processing, cancellationToken);

            var payload = new StateChanged
            {
                OrderStateOld = order.OrderState,
                OrderStateNew = OrderState.Processing,
            };

            await _orderHistoryRepository.Create(
                new QueryOrderHistoryRequest(
                    orderId,
                    payload,
                    DateTime.Now,
                    OrderHistoryItemKind.StateChanged),
                cancellationToken);
        }

        scope.Complete();
    }

    public async Task ChangeOrderStatusToCompleted(long orderId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        QueryOrdersResponse orderResponse =
            await _orderRepository.FindOrders(new QueryOrderSearchRequest(OrderId: orderId), cancellationToken);
        Order? order = orderResponse.Orders?.FirstOrDefault();
        if (order != null && order.OrderState != OrderState.Completed)
        {
            await _orderRepository.UpdateOrderState(orderId, OrderState.Completed, cancellationToken);

            var payload = new StateChanged
            {
                OrderStateOld = order.OrderState,
                OrderStateNew = OrderState.Completed,
            };
            await _orderHistoryRepository.Create(
                new QueryOrderHistoryRequest(
                    orderId,
                    payload,
                    DateTime.Now,
                    OrderHistoryItemKind.StateChanged),
                cancellationToken);
        }

        scope.Complete();
    }

    public async Task ChangeOrderStatusToCanceled(long orderId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        QueryOrdersResponse orderResponse =
            await _orderRepository.FindOrders(new QueryOrderSearchRequest(OrderId: orderId), cancellationToken);
        Order? order = orderResponse.Orders?.FirstOrDefault();
        if (order != null && order.OrderState != OrderState.Cancelled)
        {
            var payload = new StateChanged
            {
                OrderStateOld = order.OrderState,
                OrderStateNew = OrderState.Cancelled,
            };
            await _orderHistoryRepository.Create(
                new QueryOrderHistoryRequest(
                    orderId,
                    payload,
                    DateTime.Now,
                    OrderHistoryItemKind.StateChanged),
                cancellationToken);
            await _orderRepository.UpdateOrderState(orderId, OrderState.Cancelled, cancellationToken);
        }

        scope.Complete();
    }

    public async Task<QueryOrderHistoryResponse> GetOrderHistory(
        long orderId,
        OrderHistoryItemKind? kind,
        int pageSize,
        string? pageToken,
        CancellationToken cancellationToken)
    {
        QueryOrderHistoryResponse response = await
            _orderHistoryRepository.Find(
                new QueryOrderHistorySearchRequest(
                    PageSize: pageSize,
                    PageToken: pageToken,
                    OrderId: orderId,
                    Kind: kind),
                cancellationToken);
        return response;
    }
}