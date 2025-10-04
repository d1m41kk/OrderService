using System.Threading.Channels;
using Task3.ObjectModel;

namespace Task3.Implementations;

public class MessageProcessor : IMessageSender, IMessageProcessor
{
    private readonly Channel<Message> _channel;
    private readonly ICollection<IMessageHandler> _handlers;
    private readonly ProcessorOptions _options;

    public MessageProcessor(Channel<Message> channel, ICollection<IMessageHandler> handlers, ProcessorOptions options)
    {
        _channel = channel;
        _handlers = handlers;
        _options = options;
    }

    public ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await foreach (IReadOnlyList<Message> batch in _channel.Reader
                           .ReadAllAsync(cancellationToken)
                           .ChunkAsync(_options.BatchSize, _options.BatchWindow)
                           .WithCancellation(cancellationToken))
        {
            IEnumerable<Task> tasks = _handlers.Select(async handler => await handler.HandleAsync(batch, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }

    public void Complete()
    {
        _channel.Writer.TryComplete();
    }
}