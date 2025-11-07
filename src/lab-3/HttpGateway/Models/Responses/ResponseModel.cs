namespace HttpGateway.Models.Responses;

public record ResponseModel<T>(T Data, string? Message);