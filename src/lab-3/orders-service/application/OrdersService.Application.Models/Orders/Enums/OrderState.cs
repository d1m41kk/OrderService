using System.Text.Json.Serialization;

namespace OrdersService.Application.Models.Orders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderState
{
    Created,
    Processing,
    Completed,
    Cancelled,
}