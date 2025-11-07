using Grpc.Core;
using System.Text.Json;

namespace HttpGateway.Middleware;

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
        int statusCode = rpcException.StatusCode switch
        {
            StatusCode.OK => StatusCodes.Status200OK,
            StatusCode.Cancelled => StatusCodes.Status499ClientClosedRequest,
            StatusCode.Unknown => StatusCodes.Status500InternalServerError,
            StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
            StatusCode.DeadlineExceeded => StatusCodes.Status504GatewayTimeout,
            StatusCode.NotFound => StatusCodes.Status404NotFound,
            StatusCode.AlreadyExists => StatusCodes.Status409Conflict,
            StatusCode.PermissionDenied => StatusCodes.Status403Forbidden,
            StatusCode.ResourceExhausted => StatusCodes.Status429TooManyRequests,
            StatusCode.FailedPrecondition => StatusCodes.Status412PreconditionFailed,
            StatusCode.Aborted => StatusCodes.Status409Conflict,
            StatusCode.OutOfRange => StatusCodes.Status400BadRequest,
            StatusCode.Unimplemented => StatusCodes.Status501NotImplemented,
            StatusCode.Internal => StatusCodes.Status500InternalServerError,
            StatusCode.Unavailable => StatusCodes.Status503ServiceUnavailable,
            StatusCode.DataLoss => StatusCodes.Status500InternalServerError,
            StatusCode.Unauthenticated => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };
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