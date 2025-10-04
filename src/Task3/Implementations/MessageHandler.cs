using System.Text;
using Task3.ObjectModel;

namespace Task3.Implementations;

public class MessageHandler : IMessageHandler
{
    private readonly string _name;

    public MessageHandler(string name)
    {
        _name = name;
    }

    public ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        StringBuilder handlerOutput = new StringBuilder()
            .Append(_name)
            .Append(": ");
        foreach (Message message in messages)
        {
            handlerOutput.Append(message)
                .Append(", ");
        }

        Console.WriteLine(handlerOutput.ToString());
        return ValueTask.CompletedTask;
    }
}