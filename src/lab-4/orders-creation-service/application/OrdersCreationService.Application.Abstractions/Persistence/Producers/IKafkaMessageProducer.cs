using Confluent.Kafka;

namespace OrdersCreationService.Application.Abstractions.Persistence.Producers;

public interface IKafkaMessageProducer<TKey, TValue>
{
    ValueTask ProduceAsync(Message<TKey, TValue> message, CancellationToken cancellationToken);
}