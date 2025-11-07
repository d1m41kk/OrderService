using Task1Web.Domain.Enums;
using Task1Web.Domain.Models.OrderHistoryPayloads;

namespace Task1Web.DTO;

public record OrderHistoryDto(long OrderHistoryItemId,
    long OrderId,
    DateTime OrderHistoryItemCreatedAt,
    OrderHistoryItemKind OrderHistoryItemKind,
    OrderHistoryPayload? OrderHistoryItemPayload);