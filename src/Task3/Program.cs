using System.Threading.Channels;
using Task3.Implementations;
using Task3.ObjectModel;

namespace Task3;

public static class Program
{
    public static async Task Main()
    {
        var options = new ProcessorOptions
        {
            ChannelCapacity = 1000,
            BatchSize = 10,
            BatchWindow = TimeSpan.FromMilliseconds(100),
        };

        var channel = Channel.CreateBounded<Message>(options.ChannelCapacity);

        var handlers = new List<IMessageHandler>
        {
            new MessageHandler("Handler1"),
            new MessageHandler("Handler2"),
        };

        var processor = new MessageProcessor(channel, handlers, options);

        var processingTask = Task.Run(() => processor.ProcessAsync(CancellationToken.None));

        await Parallel.ForEachAsync(Enumerable.Range(1, 5), async (threadId, token) =>
        {
            for (int messageId = 1; messageId <= 20; messageId++)
            {
                var message = new Message(
                    $"Thread-{threadId}-Message-{messageId}",
                    $"Info from thread {threadId}, message {messageId}");

                await processor.SendAsync(message, token);

                await Task.Delay(10, token);
            }
        });

        Console.WriteLine("All messages sent");

        processor.Complete();

        await processingTask;

        Console.WriteLine("All messages processed");
    }
}