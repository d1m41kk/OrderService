namespace Gateway.Application.Models.Requests;

public record AddProductRequest(long OrderId, long ProductId, int Quantity);