using Task1Web.Domain.Enums;

namespace Task1Web.Domain.Models.OrderHistoryPayloads;

public class OrderCreated : OrderHistoryPayload
{
    public string CreatedBy { get; set; } = string.Empty;

    public OrderState OrderState { get; set; }
}