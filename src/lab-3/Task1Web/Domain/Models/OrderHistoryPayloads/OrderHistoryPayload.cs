using System.Text.Json.Serialization;

namespace Task1Web.Domain.Models.OrderHistoryPayloads;

[JsonDerivedType(typeof(OrderCreated), typeDiscriminator: "OrderCreated")]
[JsonDerivedType(typeof(ItemAdded), typeDiscriminator: "ItemAdded")]
[JsonDerivedType(typeof(ItemRemoved), typeDiscriminator: "ItemRemoved")]
[JsonDerivedType(typeof(StateChanged), typeDiscriminator: "StateChanged")]
public class OrderHistoryPayload
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}