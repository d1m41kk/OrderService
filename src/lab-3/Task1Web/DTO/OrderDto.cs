using Task1Web.Domain.Enums;

namespace Task1Web.DTO;

public record OrderDto(long OrderId, OrderState OrderState, DateTime OrderCreatedAt, string OrderCreatedBy);