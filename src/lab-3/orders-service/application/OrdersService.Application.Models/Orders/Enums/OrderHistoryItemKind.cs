using System.Text.Json.Serialization;

namespace OrdersService.Application.Models.Orders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderHistoryItemKind
{
    Created,
    ItemAdded,
    ItemRemoved,
    StateChanged,
}