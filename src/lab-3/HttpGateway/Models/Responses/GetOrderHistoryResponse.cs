using HttpGateway.Models.HistoryItems;

namespace HttpGateway.Models.Responses;

public record GetOrderHistoryResponse(IEnumerable<HistoryItem> History, string? NextPageToken);