using Confluent.Kafka;
using Google.Protobuf;

namespace OrdersCreationService.Application.KafkaServices;

public class ProtobufDeserializer<T> : IDeserializer<T> where T : IMessage<T>, new()
{
    private readonly MessageParser<T> _messageParser = new(() => new T());

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return _messageParser.ParseFrom(data.ToArray());
    }
}