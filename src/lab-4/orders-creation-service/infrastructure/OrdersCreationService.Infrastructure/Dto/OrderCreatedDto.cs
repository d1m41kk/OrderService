using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Infrastructure.Dto;

public class OrderCreatedDto : OrderHistoryPayloadDto
{
    public string CreatedBy { get; set; } = string.Empty;

    public OrderState OrderState { get; set; }
}