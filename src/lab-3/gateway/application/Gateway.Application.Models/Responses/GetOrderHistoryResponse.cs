using Gateway.Application.Models.HistoryItems;

namespace Gateway.Application.Models.Responses;

public record GetOrderHistoryResponse(IEnumerable<HistoryItem> History, string? NextPageToken);