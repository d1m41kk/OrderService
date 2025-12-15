namespace OrdersCreationService.Infrastructure.Dto;

public class ItemRemovedDto : OrderHistoryPayloadDto
{
    public long ProductId { get; set; }
}