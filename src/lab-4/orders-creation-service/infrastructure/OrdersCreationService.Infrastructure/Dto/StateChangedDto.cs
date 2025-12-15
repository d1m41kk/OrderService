using OrdersCreationService.Application.Models.Orders.Enums;

namespace OrdersCreationService.Infrastructure.Dto;

public class StateChangedDto : OrderHistoryPayloadDto
{
    public OrderState OrderStateOld { get; set; }

    public OrderState OrderStateNew { get; set; }
}