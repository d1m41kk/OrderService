using Grpc.Core;
using System.Text.Json;

namespace Gateway.Presentation.Middleware;

public class GrpcExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GrpcExceptionHandler> _logger;

    public GrpcExceptionHandler(RequestDelegate next, ILogger<GrpcExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException rpcException)
        {
            _logger.LogError(rpcException.Message);
            await HandleException(context, rpcException);
        }
    }

    private static async Task HandleException(HttpContext context, RpcException rpcException)
    {
        int statusCode;

        if (rpcException.StatusCode == StatusCode.InvalidArgument)
        {
            statusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        var errorResponse = new
        {
            error = rpcException.StatusCode.ToString(),
            message = rpcException.Status.Detail,
            grpc_code = (int)rpcException.StatusCode,
            http_code = statusCode,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}