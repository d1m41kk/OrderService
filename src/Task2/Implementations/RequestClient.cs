using System.Collections.Concurrent;
using Task2.LibraryWithReactiveApproach;

namespace Task2.Implementations;

public class RequestClient : ILibraryOperationHandler, IRequestClient
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResponseModel>> _requests = new();
    private readonly ConcurrentDictionary<Guid, CancellationTokenRegistration> _registrations = new();
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

            if (_registrations.TryRemove(requestId, out CancellationTokenRegistration registration))
            {
                registration.Dispose();
            }
        }
    }

    public void HandleOperationError(Guid requestId, Exception exception)
    {
        if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? tcs))
        {
            tcs.TrySetException(exception);
            if (_registrations.TryRemove(requestId, out CancellationTokenRegistration registration))
            {
                registration.Dispose();
            }
        }
    }

    public Task<ResponseModel> SendAsync(RequestModel request, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        var tcs = new TaskCompletionSource<ResponseModel>();
        _requests.TryAdd(requestId, tcs);
        CancellationTokenRegistration registration = cancellationToken.Register(() =>
        {
            if (_requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? pending))
            {
                pending.TrySetCanceled(cancellationToken);
            }

            if (_registrations.TryRemove(requestId, out CancellationTokenRegistration reg))
            {
                reg.Dispose();
            }
        });
        _registrations.TryAdd(requestId, registration);

        if (cancellationToken.IsCancellationRequested)
        {
            _requests.TryRemove(requestId, out TaskCompletionSource<ResponseModel>? pending);
            if (_registrations.TryRemove(requestId, out CancellationTokenRegistration toDispose))
            {
                toDispose.Dispose();
            }

            pending?.TrySetCanceled(cancellationToken);

            return tcs.Task;
        }

        try
        {
            _libraryOperationService.BeginOperation(requestId, request, cancellationToken);
        }
        catch (Exception e)
        {
            _requests.TryRemove(requestId, out _);
            if (_registrations.TryRemove(requestId, out CancellationTokenRegistration reg))
            {
                reg.Dispose();
            }

            tcs.TrySetException(e);
        }

        return tcs.Task;
    }
}