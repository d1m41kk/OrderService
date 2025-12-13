using Confluent.Kafka;
using Orders.Kafka.Contracts;
using OrdersCreationService.Application.Abstractions.Persistence.Producers;
using OrdersCreationService.Application.Abstractions.Persistence.Queries;
using OrdersCreationService.Application.Abstractions.Persistence.Repositories;
using OrdersCreationService.Application.Models.Orders;
using OrdersCreationService.Application.Models.Orders.Enums;
using OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads;
using System.Transactions;
using OrderCreated = OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads.OrderCreated;

namespace OrdersCreationService.Application.Orders;

public class OrdersCreatingService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderHistoryRepository _orderHistoryRepository;
    private readonly IKafkaMessageProducer<OrderCreationKey, OrderCreationValue> _kafkaMessageProducer;

    public OrdersCreatingService(
        IOrderRepository orderRepository,
        IOrderHistoryRepository orderHistoryRepository,
        IKafkaMessageProducer<OrderCreationKey, OrderCreationValue> kafkaMessageProducer)
    {
        _orderRepository = orderRepository;
        _orderHistoryRepository = orderHistoryRepository;
        _kafkaMessageProducer = kafkaMessageProducer;
    }

    public async Task<OrderState> GetOrderState(long orderId, CancellationToken cancellationToken)
    {
        QueryOrdersResponse orderResponse = await _orderRepository.FindOrders(new QueryOrderSearchRequest(OrderId: orderId), cancellationToken);
        if (orderResponse.Orders != null)
        {
            Order order = orderResponse.Orders.FirstOrDefault() ?? throw new Exception("Order not found");
            return order.OrderState;
        }

        throw new Exception("Order not found");
    }

    public async Task<long> CreateOrder(string orderCreatedBy, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        DateTime orderCreatedAt = DateTime.UtcNow;

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

        var message = new Message<OrderCreationKey, OrderCreationValue>()
        {
            Key = new OrderCreationKey
            {
                OrderId = order.OrderId,
            },
            Value = new OrderCreationValue
            {
                OrderCreated = new OrderCreationValue.Types.OrderCreated
                {
                    OrderId = order.OrderId,
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(orderCreatedAt),
                },
            },
            Timestamp = new Timestamp(DateTime.UtcNow),
        };
        await _kafkaMessageProducer.ProduceAsync(message, cancellationToken);
        return order.OrderId;
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

        if (order != null)
        {
            var message = new Message<OrderCreationKey, OrderCreationValue>()
            {
                Key = new OrderCreationKey()
                {
                    OrderId = orderId,
                },
                Value = new OrderCreationValue
                {
                    OrderProcessingStarted = new OrderCreationValue.Types.OrderProcessingStarted
                    {
                        OrderId = order.OrderId,
                        StartedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                    },
                },
                Timestamp = new Timestamp(DateTime.UtcNow),
            };
            await _kafkaMessageProducer.ProduceAsync(message, cancellationToken);
        }
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
}