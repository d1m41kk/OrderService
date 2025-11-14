namespace OrdersService.Infrastructure.Dto;

public class ItemAddedDto : OrderHistoryPayloadDto
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}