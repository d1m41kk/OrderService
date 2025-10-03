using NSubstitute;
using Task2.Implementations;
using Task2.LibraryWithReactiveApproach;
using Xunit;

namespace Lab1.Tests;

public class RequestClientTests
{
    private readonly ILibraryOperationService _libraryOperationService;
    private readonly RequestClient _requestClient;

    public RequestClientTests()
    {
        _libraryOperationService = Substitute.For<ILibraryOperationService>();
        _requestClient = new RequestClient(_libraryOperationService);
    }

    [Fact]
    public async Task SendAsync_WhenHandleOperationResultCalled_ReturnsResult()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        byte[] responseData = [4, 5, 6];
        CancellationToken cancellationToken = CancellationToken.None;

        Guid capturedRequestId = Guid.Empty;
        _libraryOperationService
            .When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo => capturedRequestId = callInfo.Arg<Guid>());

        Task<ResponseModel> task = _requestClient.SendAsync(request, cancellationToken);

        _requestClient.HandleOperationResult(capturedRequestId, responseData);

        ResponseModel result = await task;

        Assert.Equal(responseData, result.Data);
    }

    [Fact]
    public async Task SendAsync_WhenHandleOperationErrorCalled_ThrowsException()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        var expectedException = new InvalidOperationException("Operation failed");
        CancellationToken cancellationToken = CancellationToken.None;

        Guid capturedRequestId = Guid.Empty;
        _libraryOperationService
            .When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo => capturedRequestId = callInfo.Arg<Guid>());

        Task<ResponseModel> task = _requestClient.SendAsync(request, cancellationToken);

        _requestClient.HandleOperationError(capturedRequestId, expectedException);

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Equal(expectedException.Message, actualException.Message);
    }

    [Fact]
    public async Task SendAsync_WithAlreadyCancelledToken_ThrowsTaskCanceledException()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _requestClient.SendAsync(request, cts.Token));

        _libraryOperationService.DidNotReceive()
            .BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_WhenTokenCancelledAfterCall_ThrowsTaskCanceledException()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        using var cts = new CancellationTokenSource();

        _libraryOperationService
            .When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo => callInfo.Arg<Guid>());

        Task<ResponseModel> task = _requestClient.SendAsync(request, cts.Token);

        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }

    [Fact]
    public async Task SendAsync_WhenBeginOperationCallsHandleOperationResultSynchronously_ReturnsResult()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        byte[] responseData = [4, 5, 6];
        CancellationToken cancellationToken = CancellationToken.None;

        _libraryOperationService
            .When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                Guid requestId = callInfo.Arg<Guid>();
                _requestClient.HandleOperationResult(requestId, responseData);
            });

        ResponseModel result = await _requestClient.SendAsync(request, cancellationToken);

        Assert.Equal(responseData, result.Data);
    }

    [Fact]
    public async Task SendAsync_WhenBeginOperationCallsHandleOperationErrorSynchronously_ThrowsException()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        var expectedException = new InvalidOperationException("Synchronous error");
        CancellationToken cancellationToken = CancellationToken.None;

        _libraryOperationService
            .When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                Guid requestId = callInfo.Arg<Guid>();
                _requestClient.HandleOperationError(requestId, expectedException);
            });

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _requestClient.SendAsync(request, cancellationToken));

        Assert.Equal(expectedException.Message, actualException.Message);
    }

    [Fact]
    public async Task SendAsync_WhenBeginOperationCancelsTokenSynchronously_ThrowsTaskCanceledException()
    {
        var request = new RequestModel(string.Empty, [1, 2, 3]);
        using var cts = new CancellationTokenSource();

        _libraryOperationService
            .When(x => x.BeginOperation(Arg.Any<Guid>(), Arg.Any<RequestModel>(), Arg.Any<CancellationToken>()))
            .Do(_ => cts.Cancel());

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _requestClient.SendAsync(request, cts.Token));
    }
}