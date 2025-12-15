using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Orders.Kafka.Contracts;
using OrdersCreationService.Application.Abstractions.Persistence.Producers;
using OrdersCreationService.Application.Models.KafkaOptions;
using System.Threading.Channels;

namespace OrdersCreationService.Application.KafkaServices;

public class KafkaOrderCreationProducer : IKafkaMessageProducer<OrderCreationKey, OrderCreationValue>, IDisposable
{
    private readonly ProducerOptions _options;
    private readonly IProducer<OrderCreationKey, OrderCreationValue> _producer;
    private readonly Channel<Message<OrderCreationKey, OrderCreationValue>> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public KafkaOrderCreationProducer(IOptions<ProducerOptions> producerOptions)
    {
        _options = producerOptions.Value;
        _channel = Channel.CreateBounded<Message<OrderCreationKey, OrderCreationValue>>(
            new BoundedChannelOptions(_options.BatchSize)
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = _options.Acks,
            BatchSize = _options.BatchSize,
        };
        _producer = new ProducerBuilder<OrderCreationKey, OrderCreationValue>(config)
            .SetKeySerializer(new ProtobufSerializer<OrderCreationKey>())
            .SetValueSerializer(new ProtobufSerializer<OrderCreationValue>())
            .Build();
        Task.Run(ProduceBathToTopicAsync);
    }

    public ValueTask ProduceAsync(
        Message<OrderCreationKey, OrderCreationValue> message,
        CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _channel.Writer.Complete();
        _producer.Flush();
        _producer.Dispose();
        _cancellationTokenSource.Dispose();
    }

    private async Task ProduceBathToTopicAsync()
    {
        await foreach (Message<OrderCreationKey, OrderCreationValue> message in _channel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            await _producer.ProduceAsync(_options.Topic, message, _cancellationTokenSource.Token);
        }
    }
}