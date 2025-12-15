using Confluent.Kafka;
using Google.Protobuf;

namespace OrdersCreationService.Application.KafkaServices;

public class ProtobufSerializer<T> : ISerializer<T> where T : IMessage<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return data?.ToByteArray() ?? [];
    }
}