using Confluent.Kafka;

namespace OrdersCreationService.Application.Models.KafkaOptions;

public class ProducerOptions
{
    public string BootstrapServers { get; set; } = string.Empty;

    public Acks? Acks { get; set; }

    public string Topic { get; set; } = string.Empty;

    public int BatchSize { get; set; }
}