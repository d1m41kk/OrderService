using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orders.Kafka.Contracts;
using OrdersCreationService.Application.Abstractions.Persistence.Consumers;
using OrdersCreationService.Application.KafkaHandlers;
using OrdersCreationService.Application.Models.KafkaOptions;
using System.Threading.Channels;

namespace OrdersCreationService.Application.KafkaServices;

public class KafkaOrderCreationConsumer : BackgroundService,
    IKafkaMessageConsumer<OrderProcessingKey, OrderProcessingValue>
{
    private readonly OrderProcessingEventsHandler _orderProcessingEventsHandler;
    private readonly ConsumerOptions _consumerOptions;
    private readonly IConsumer<OrderProcessingKey, OrderProcessingValue> _consumer;
    private readonly Channel<ConsumeResult<OrderProcessingKey, OrderProcessingValue>> _channel;
    private readonly Task _consumerTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public KafkaOrderCreationConsumer(
        OrderProcessingEventsHandler orderProcessingEventsHandler,
        IOptions<ConsumerOptions> consumerOptions)
    {
        _orderProcessingEventsHandler = orderProcessingEventsHandler;
        _consumerOptions = consumerOptions.Value;
        _channel = Channel.CreateBounded<ConsumeResult<OrderProcessingKey, OrderProcessingValue>>(
            new BoundedChannelOptions(_consumerOptions.BatchSize)
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });
        var config = new ConsumerConfig
        {
            GroupId = _consumerOptions.GroupId,
            BootstrapServers = _consumerOptions.BootstrapServers,
        };
        _consumer = new ConsumerBuilder<OrderProcessingKey, OrderProcessingValue>(config)
            .SetKeyDeserializer(new ProtobufDeserializer<OrderProcessingKey>())
            .SetValueDeserializer(new ProtobufDeserializer<OrderProcessingValue>())
            .Build();
        _consumerTask = Task.Run(ConsumeBatchAsync);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumerTask.WaitAsync(cancellationToken);
        _channel.Writer.Complete();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        _channel.Writer.Complete();
        _cancellationTokenSource.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_consumerOptions.Topic);
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ConsumeResult<OrderProcessingKey, OrderProcessingValue>
                    consumeResult = _consumer.Consume(stoppingToken);
                await _channel.Writer.WriteAsync(consumeResult, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ConsumeBatchAsync()
    {
        await foreach (ConsumeResult<OrderProcessingKey, OrderProcessingValue> consumeResult in _channel.Reader
                           .ReadAllAsync(_cancellationTokenSource.Token))
        {
            await _orderProcessingEventsHandler.HandleAsync(consumeResult.Message.Key, consumeResult.Message.Value, _cancellationTokenSource.Token);
            _consumer.Commit(consumeResult);
        }
    }
}