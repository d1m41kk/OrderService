using OrdersService.Application.Models.Orders.Enums;

namespace OrdersService.Infrastructure.Dto;

public class StateChangedDto : OrderHistoryPayloadDto
{
    public OrderState OrderStateOld { get; set; }

    public OrderState OrderStateNew { get; set; }
}