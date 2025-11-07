namespace HttpGateway.Models.Requests;

public record AddProductRequest(long OrderId, long ProductId, int Quantity);