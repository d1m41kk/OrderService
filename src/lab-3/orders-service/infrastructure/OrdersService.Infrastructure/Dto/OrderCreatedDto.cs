using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Infrastructure.Dto;

public class OrderCreatedDto : OrderHistoryPayloadDto
{
    public string CreatedBy { get; set; } = string.Empty;

    public OrderState OrderState { get; set; }
}