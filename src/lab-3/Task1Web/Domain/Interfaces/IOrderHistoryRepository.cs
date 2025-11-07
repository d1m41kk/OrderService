using Task1Web.Domain.Enums;
using Task1Web.Domain.Models;
using Task1Web.Domain.Models.OrderHistoryPayloads;

namespace Task1Web.Domain.Interfaces;

public interface IOrderHistoryRepository
{
    Task<QueryOrderHistoryResponse> Find(
        long? orderId,
        OrderHistoryItemKind? orderHistoryItemKind,
        int pageSize,
        string? pageToken);

    Task CreateHistoryItem(
        long orderId,
        DateTime orderHistoryItemCreatedAt,
        OrderHistoryItemKind orderHistoryItemKind,
        OrderHistoryPayload orderHistoryItemPayload);
}