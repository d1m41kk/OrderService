using Task1Web.Domain.Enums;

namespace HttpGateway.Models.Requests;

public record GetOrderHistoryRequest(long OrderId, OrderHistoryItemKind? Kind, int PageSize, string? PageToken);