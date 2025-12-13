namespace OrdersCreationService.Application.Models.Orders.OrderHistoryPayloads;

public class OrderHistoryPayload
{
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}