using Grpc.Core;
using Grpc.Core.Interceptors;

namespace OrdersService.Presentation.Grpc.Interceptors;

public class RequestInterceptor : Interceptor
{
    private readonly ILogger _logger;

    public RequestInterceptor(ILogger logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Server interceptor started. Type: {Type}, Method: {Method}", MethodType.Unary, context.Method);
        try
        {
            return await continuation(request, context);
        }
        catch (Exception)
        {
            _logger.LogError($"Error thrown by {context.Method}");
            throw;
        }
    }
}