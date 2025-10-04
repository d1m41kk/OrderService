using System.Collections.Concurrent;
using Task2.LibraryWithReactiveApproach;

namespace Task2.Implementations;

public class RequestClient : ILibraryOperationHandler, IRequestClient
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _requests = new();
    private readonly ILibraryOperationService _libraryOperationService;

    public RequestClient(ILibraryOperationService libraryOperationService)
    {
        _libraryOperationService = libraryOperationService;
    }

    public void HandleOperationResult(Guid requestId, byte[] data)
    {
        if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            var responce = new ResponseModel(data);
            tcs.TrySetResult(responce);
        }
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            tcs.TrySetException(exception);
        }
    }

    public async Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ResponseModel>();

        await using CancellationTokenRegistration registration = cancellationToken.Register(() =>
        {
            if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? pending))
            {
                pending.TrySetCanceled(cancellationToken);
            }
        });

        if (cancellationToken.IsCancellationRequested)
        {
            _requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? _);
            tcs.TrySetCanceled(cancellationToken);
            return await tcs.Task;
        }

        _requests.TryAdd(requestId, tcs);

        try
        {
            _libraryOperationService.BeginOperation(requestId, request, cancellationToken);
        }
        catch (Exception e)
        {
            HandleOperationError(requestId, e);
        }

        return await tcs.Task;
    }
}