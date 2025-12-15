using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace OrdersCreationService.Application.Grpc.Interceptors;

public class RequestInterceptor : Interceptor
{
    private readonly ILogger<RequestInterceptor> _logger;

    public RequestInterceptor(ILogger<RequestInterceptor> logger)
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