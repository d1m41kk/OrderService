using Task3.Models;

namespace Task3.Handlers;

public abstract class Handler
{
    public Handler? Next { get; set; }

    protected Handler(Handler? next)
    {
        Next = next;
    }

    public abstract Task Handle(DisplayInfo displayInfo);
}