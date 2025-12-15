using System.Text.Json.Serialization;

namespace OrdersCreationService.Application.Models.Orders.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderState
{
    Created,
    Processing,
    Completed,
    Cancelled,
}