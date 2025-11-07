namespace Task1Web.Presentation.Grpc.Models;

public class ErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}