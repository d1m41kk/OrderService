namespace OrdersCreationService.Application.Models.KafkaOptions;

public class ConsumerOptions
{
    public string GroupId { get; set; } = string.Empty;

    public string BootstrapServers { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public int BatchSize { get; set; }
}