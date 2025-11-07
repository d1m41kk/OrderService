using System.Transactions;
using Task1Web.Domain.Enums;
using Task1Web.Domain.Interfaces;
using Task1Web.Domain.Models;
using Task1Web.Domain.Models.OrderHistoryPayloads;

namespace Task1Web.Domain.Services;

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

    public async Task<long> CreateOrder(string orderCreatedBy)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        DateTime orderCreatedAt = DateTime.Now;

        long id = await _orderRepository.CreateOrder(OrderState.Created, orderCreatedAt, orderCreatedBy);

        var payload = new OrderCreated
        {
            CreatedBy = orderCreatedBy,
            CreatedAt = orderCreatedAt,
        };

        await _orderHistoryRepository.CreateHistoryItem(
            id,
            orderCreatedAt,
            OrderHistoryItemKind.Created,
            payload);
        scope.Complete();
        return id;
    }

    public async Task AddProductToOrder(long orderId, long productId, int orderItemQuantity)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        QueryProductsResponse productsResponse =
            await _productRepository.FindProducts(productId, null, null, null, 1, null);
        if (productsResponse.Products != null)
        {
            QueryOrdersResponse ordersResponse =
                await _orderRepository.FindOrders(orderId, OrderState.Created, string.Empty, 1, null);
            if (ordersResponse.Orders != null)
            {
                await _orderItemRepository.CreateOrderItem(orderId, productId, orderItemQuantity);

                var payload = new ItemAdded
                {
                    ProductId = productId,
                    Quantity = orderItemQuantity,
                    CreatedAt = DateTime.Now,
                };
                await _orderHistoryRepository.CreateHistoryItem(
                    orderId,
                    DateTime.Now,
                    OrderHistoryItemKind.ItemAdded,
                    payload);
            }
        }

        scope.Complete();
    }

    public async Task DeleteProductInOrder(long orderId, long productId)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        QueryProductsResponse productsResponse =
            await _productRepository.FindProducts(productId, null, null, null, 1, null);
        if (productsResponse.Products != null)
        {
            QueryOrdersResponse ordersResponse =
                await _orderRepository.FindOrders(orderId, OrderState.Created, string.Empty, 1, null);
            if (ordersResponse.Orders != null)
            {
                QueryOrderItemsResponse itemsResponse =
                    await _orderItemRepository.FindOrderItems(orderId, productId, false, 1, null);
                OrderItem? item = itemsResponse.OrderItems?.FirstOrDefault();
                if (item != null)
                {
                    await _orderItemRepository.SoftDeleteOrderItem(item.OrderItemId);
                    var payload = new ItemRemoved
                    {
                        ProductId = productId,
                        CreatedAt = DateTime.Now,
                    };
                    await _orderHistoryRepository.CreateHistoryItem(
                        orderId,
                        DateTime.Now,
                        OrderHistoryItemKind.ItemRemoved,
                        payload);
                }
            }
        }

        scope.Complete();
    }

    public async Task ChangeOrderStatusToProcessing(long orderId)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        QueryOrdersResponse orderResponse = await _orderRepository.FindOrders(orderId, null, string.Empty, 1, null);
        Order? order = orderResponse.Orders?.FirstOrDefault();
        if (order != null && order.OrderState != OrderState.Processing)
        {
            await _orderRepository.UpdateOrderState(orderId, OrderState.Processing);

            var payload = new StateChanged
            {
                OrderStateOld = order.OrderState,
                OrderStateNew = OrderState.Processing,
            };
            await _orderHistoryRepository.CreateHistoryItem(
                orderId,
                DateTime.Now,
                OrderHistoryItemKind.StateChanged,
                payload);
        }

        scope.Complete();
    }

    public async Task ChangeOrderStatusToCompleted(long orderId)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        QueryOrdersResponse orderResponse = await _orderRepository.FindOrders(orderId, null, string.Empty, 1, null);
        Order? order = orderResponse.Orders?.FirstOrDefault();
        if (order != null && order.OrderState != OrderState.Completed)
        {
            await _orderRepository.UpdateOrderState(orderId, OrderState.Completed);

            var payload = new StateChanged
            {
                OrderStateOld = order.OrderState,
                OrderStateNew = OrderState.Completed,
            };
            await _orderHistoryRepository.CreateHistoryItem(
                orderId,
                DateTime.Now,
                OrderHistoryItemKind.StateChanged,
                payload);
        }

        scope.Complete();
    }

    public async Task ChangeOrderStatusToCanceled(long orderId)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        QueryOrdersResponse orderResponse = await _orderRepository.FindOrders(orderId, null, string.Empty, 1, null);
        Order? order = orderResponse.Orders?.FirstOrDefault();
        if (order != null && order.OrderState != OrderState.Cancelled)
        {
            var payload = new StateChanged
            {
                OrderStateOld = order.OrderState,
                OrderStateNew = OrderState.Cancelled,
            };
            await _orderHistoryRepository.CreateHistoryItem(
                orderId,
                DateTime.Now,
                OrderHistoryItemKind.StateChanged,
                payload);
            await _orderRepository.UpdateOrderState(orderId, OrderState.Cancelled);
        }

        scope.Complete();
    }

    public async Task<QueryOrderHistoryResponse> GetOrderHistory(
        long orderId,
        OrderHistoryItemKind? kind,
        int pageSize,
        string? pageToken)
    {
        Task<QueryOrderHistoryResponse> response = _orderHistoryRepository.Find(orderId, kind, pageSize, pageToken);
        return await response;
    }
}