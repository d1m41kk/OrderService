using System.Text.Json.Serialization;

namespace OrdersCreationService.Infrastructure.Dto;

[JsonDerivedType(typeof(OrderCreatedDto), typeDiscriminator: "OrderCreated")]
[JsonDerivedType(typeof(ItemAddedDto), typeDiscriminator: "ItemAdded")]
[JsonDerivedType(typeof(ItemRemovedDto), typeDiscriminator: "ItemRemoved")]
[JsonDerivedType(typeof(StateChangedDto), typeDiscriminator: "StateChanged")]
public class OrderHistoryPayloadDto
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}