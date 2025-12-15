namespace OrdersService.Infrastructure.Dto;

public class ItemRemovedDto : OrderHistoryPayloadDto
{
    public long ProductId { get; set; }
}